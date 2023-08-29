using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Native;
using Xunit.Abstractions;

namespace PactExploration.Tests
{
    public class ProviderTests
    {
        private string pactServiceUri = "http://127.0.0.1:9002";
        private string providerUri = "http://127.0.0.1:9003";
        //private string pactServiceUri = "http://postman-echo.com";

        private ITestOutputHelper outputHelper;

        public ProviderTests(ITestOutputHelper output)
        {
            this.outputHelper = output;
        }

        //[Fact]
        public void ValidatePostmanApiPact()
        {
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput> { new XUnitOutput(this.outputHelper)}//Xunit n consegue ler o erro q o PactNet loga. é necessário ter um custom só pra fazer o WriteLine dos erros.
            };
               
            //esse webHost sobe a aplicação pra executar os testes. tem casos q é possível rodar teste no provedor sem subir a aplicação, outros não. não entendi ainda quando usar uma maneira ou outra.
            //using (var webHost = WebHost.CreateDefaultBuilder().UseStartup<Startup>().UseUrls(this.pactServiceUri).Build())
            //{
            //    webHost.Start();

                //var pactOptions = new PactUriOptions("faM71GPVLZkuKYPcRMYo2g");
                var pactFile = new FileInfo("../../../../pacts/WeConsumingSomeone-PostmanApi.json");

                IPactVerifier pactVerifier = new PactVerifier(config);
                pactVerifier
                    .FromPactFile(pactFile)
                    //.FromPactBroker(new Uri("https://stonepagamentos.pactflow.io"), pactOptions) //se a gente estiver usando um broker
                    //.FromPactUri() se a gente tiver um repositório online pra compartilhar contratos, feito por nós mesmos
                    .WithProviderStateUrl(new Uri($"{pactServiceUri}/provider-states"))
                    .ServiceProvider("PostmanApi", new Uri(providerUri))
                    .HonoursPactWith("WeConsumingSomeone")//não entendi, o parâmetro é consumerName, mas só funciona se eu passar o nome do provedor q é o PostmanApi
                    .Verify();
            //}
        }
    }
}