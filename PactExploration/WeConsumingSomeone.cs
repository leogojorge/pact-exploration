using Newtonsoft.Json;
using System.Text;
using System;
using PactExploration.Models;

namespace PactExploration
{
    public class WeConsumingSomeone
    {
        private readonly HttpClient ProviderClient; //http://localhost:5267

        public WeConsumingSomeone(Uri uri)
        {
            this.ProviderClient = new HttpClient();
            this.ProviderClient.BaseAddress = uri;
        }

        public async Task<HttpResponseMessage> GetSomeData()
        {
            var response = await this.ProviderClient.GetAsync("api/some-data");

            return response;
        }

        public async Task<HttpResponseMessage> GetSomeDataById(string id)
        {
            var response = await this.ProviderClient.GetAsync($"/api/some-data/{id}");

            return response;
        }

        public async Task<HttpResponseMessage> PostSomeData(ProviderPostRequest request)
        {
            var requestAsJson = JsonConvert.SerializeObject(request);
            var data = new StringContent(requestAsJson, Encoding.UTF8, "application/json");

            var response = await this.ProviderClient.PostAsync("/api/some-data", data);

            return response;
        }

        public async Task<HttpResponseMessage> DeleteSomeData(string id)
        {
            var response = await this.ProviderClient.DeleteAsync($"/api/some-data/{id}");

            return response;
        }
    }
}