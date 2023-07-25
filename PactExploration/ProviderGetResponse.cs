using Newtonsoft.Json;

namespace PactExploration
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ProviderGetResponse
    {
        public Args args { get; set; }
        public Headers headers { get; set; }
        public string url { get; set; }
    }
    public class Args
    {
    }

    public class Headers
    {
        [JsonProperty("x-forwarded-proto")]
        public string xforwardedproto { get; set; }

        [JsonProperty("x-forwarded-port")]
        public string xforwardedport { get; set; }
        public string host { get; set; }

        //this field is dynamic
        //[JsonProperty("x-amzn-trace-id")]
        //public string xamzntraceid { get; set; }

        [JsonProperty("user-agent")]
        public string useragent { get; set; }
        public string accept { get; set; }

        [JsonProperty("cache-control")]
        public string cachecontrol { get; set; }

        //this field is dynamic
        //[JsonProperty("postman-token")]
        //public string postmantoken { get; set; }

        [JsonProperty("accept-encoding")]
        public string acceptencoding { get; set; }
    }
}
