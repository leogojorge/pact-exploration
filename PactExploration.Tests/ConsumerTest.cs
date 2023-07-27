using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PactExploration;
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
        private ProviderGetResponse responseBody;

        public ConsumerTest(ITestOutputHelper output)
        {
            responseBody = new ProviderGetResponse()
            {
                url = "http://postman-echo.com/get",
                args = new(),
                headers = new()
                {
                    xforwardedport = "80",
                    xforwardedproto = "http",
                    host = "postman-echo.com",
                    useragent = "PostmanRuntime/7.32.3",
                    accept = "*/*",
                    cachecontrol = "no-cache",
                    acceptencoding = "gzip, deflate, br",
                }
            };

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
            pact = Pact.V3("WeConsumingSomeone", "PostmanApi", config).UsingNativeBackend(9000);
        }

        [Fact]
        public async void ValidGetOnPostmanApi()
        {
            // Arange
            pact.UponReceiving("A get for the Postmang/Get API (our provider)")
                .Given("All seted up properlly") //essa string se transforma no ProviderState quando o teste chega no lado do provedor. ele é usado pra poder testar um endpoint com cenários diferentes. ex: get usuário GIVEN usuário inexistente.
                .WithRequest(HttpMethod.Get, "/get")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new TypeMatcher(responseBody));

            //act assert
            await pact.VerifyAsync(async context =>
            {
                this.WeConsumingSomeone = new WeConsumingSomeone(context.MockServerUri); //mermão, se a url da api for diferente da url q o Pact usa,o teste quebra, pois o Pact entende que são serviços diferentes, então ele considera que o serviço que ele está olhando não recebeu nenhuma requisição
                var response = await WeConsumingSomeone.GetSomeData();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            });
        }

        [Fact]
        public async void InvalidPathOnPostmanApi()
        {
            //arrange
            pact.UponReceiving("A invalid path for PostmanApi")
                .Given("All seted up properlly")
                .WithRequest(HttpMethod.Get, "/get")
            .WillRespond()
                .WithStatus(HttpStatusCode.NotFound)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(null);

            await pact.VerifyAsync(async context => 
            {
                this.WeConsumingSomeone = new WeConsumingSomeone(context.MockServerUri);
                var response = await this.WeConsumingSomeone.GetSomeData();
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            });
        }
    }
}
