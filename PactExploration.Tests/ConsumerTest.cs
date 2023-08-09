using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactExploration;
using PactExploration.Models;
using PactExploration.Tests;
using PactNet;
using PactNet.Matchers;
using PactNet.Native;
using System.Net;
using Xunit.Abstractions;

namespace TestProject1
{
    public class ConsumerTest
    {
        private WeConsumingSomeone WeConsumingSomeone;
        private IPactBuilderV3 pact;

        public ConsumerTest(ITestOutputHelper output)
        {
            var config = new PactConfig
            {
                PactDir = "../../../../pacts",
                Outputters = new[] { new XUnitOutput(output) },
                LogDir = "pact_logs",
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            // Initialize Rust backend
            pact = Pact.V3("WeConsumingSomeone", "SomeProvider", config).UsingNativeBackend(9000); //sobe um server mock
        }

        [Fact]
        public async void ValidGetOnProviderApi()
        {
            // Arange
            var expectedResponse = GeneretaGetSomeDataResponse();

            pact.UponReceiving("A get an entity from a provider")
                .Given("2 initial products created") //essa string se transforma no ProviderState quando o teste chega no lado do provedor. ele é usado pra poder testar um endpoint com cenários diferentes. ex: get usuário GIVEN usuário inexistente.
                .WithRequest(HttpMethod.Get, "/api/some-data")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new TypeMatcher(expectedResponse));

            //act assert
            await pact.VerifyAsync(async context =>
            {
                this.WeConsumingSomeone = new WeConsumingSomeone(context.MockServerUri); //mermão, se a url da api for diferente da url q o Pact usa,o teste quebra, pois o Pact entende que são serviços diferentes, então ele considera que o serviço que ele está olhando não recebeu nenhuma requisição
                var response = await WeConsumingSomeone.GetSomeData();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void Validate_GetProductById_WithExistingId_OnProviderApi()
        {
            // Arange
            var expectedResponse = GeneretaGetSomeDataResponse()[1];
            Guid idPersisted = new Guid("993a1ad5-7f7a-4a91-91fb-c0ee62755a2d");

            pact.UponReceiving("A get entity by id from a provider")
                .Given("Products with id 993a1ad5-7f7a-4a91-91fb-c0ee62755a2d exist")
                .WithRequest(HttpMethod.Get, $"/api/some-data/{idPersisted}")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new TypeMatcher(expectedResponse));

            //act assert
            await pact.VerifyAsync(async context =>
            {
                this.WeConsumingSomeone = new WeConsumingSomeone(context.MockServerUri); //mermão, se a url da api for diferente da url q o Pact usa,o teste quebra, pois o Pact entende que são serviços diferentes, então ele considera que o serviço que ele está olhando não recebeu nenhuma requisição
                var response = await WeConsumingSomeone.GetSomeDataById(idPersisted.ToString());
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void Validate_GetProductById_WithNonExistantId_OnProviderApi()
        {
            // Arange
            var expectedResponse = GeneretaGetSomeDataResponse();
            Guid idNotPersisted = new Guid("8539cfbb-32d8-49c2-9c33-01f8159a1dae");

            pact.UponReceiving("A get entity by id from a provider")
                .Given("Id not stored")
                .WithRequest(HttpMethod.Get, $"/api/some-data/{idNotPersisted}")
            .WillRespond()
                .WithStatus(HttpStatusCode.NoContent)
                .WithHeader("Content-Length", "0");

            //act assert
            await pact.VerifyAsync(async context =>
            {
                this.WeConsumingSomeone = new WeConsumingSomeone(context.MockServerUri); //mermão, se a url da api for diferente da url q o Pact usa,o teste quebra, pois o Pact entende que são serviços diferentes, então ele considera que o serviço que ele está olhando não recebeu nenhuma requisição
                var response = await WeConsumingSomeone.GetSomeDataById(idNotPersisted.ToString());
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            });
        }

        private List<ProviderGetSomeDataByIdResponse> GeneretaGetSomeDataResponse()
        {
            return new List<ProviderGetSomeDataByIdResponse>()
            {
                new()
                {
                    Id = new Guid("993a1ad5-7f7a-4a91-91fb-c0ee62755a2d"),
                    Name = "Name1",
                    LastName = "LastName1",
                    Age = "Age1",
                },
                new()
                {
                    Id = new Guid("8518eb80-c2c4-4c1f-8573-905af0d7f3e2"),
                    Name = "Name2",
                    LastName = "LastName2",
                    Age = "Age3",
                }
            };
        }

    }
}
