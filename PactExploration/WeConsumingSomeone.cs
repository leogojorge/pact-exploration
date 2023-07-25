namespace PactExploration
{
    public class WeConsumingSomeone
    {
        private readonly Uri PostmanBaseUri; //our provider  = "http://postman-echo.com"

        public WeConsumingSomeone(Uri uri)
        {
            this.PostmanBaseUri = uri;
        }

        public async Task<HttpResponseMessage> GetSomeData()
        {
            using (var client = new HttpClient { BaseAddress = this.PostmanBaseUri })
            {
                try
                {
                    var response = await client.GetAsync("/get");
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("There was a problem connecting to Postman API.", ex);
                }
            }
        }
    }
}