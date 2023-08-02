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
                DefaultJsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }
            };

            // Initialize Rust backend
            pact = Pact.V3("WeConsumingSomeone", "SomeProvider", config).UsingNativeBackend(9000); //sobe um server mock
        }

        [Fact]
        public async void ValidGetOnPostmanApi()
        {
            // Arange
            var expectedResponse = GeneretaGetSomeDataResponse();

            pact.UponReceiving("A get an entity from a provider")
                .Given("It already has 2 entities") //essa string se transforma no ProviderState quando o teste chega no lado do provedor. ele é usado pra poder testar um endpoint com cenários diferentes. ex: get usuário GIVEN usuário inexistente.
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
