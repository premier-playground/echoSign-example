using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace echoSign_example.Controllers
{
    public class EchoSignController : ApiController
    {
        /*// GET: echoSign
        public string Index()
        {
            return "test";
        }

        // GET: oauthDemo
        public string GetCodeAndState(string code, string state)
        {
            return "teste";
        }*/

        private static HttpClient _httpClient = new HttpClient();

        [HttpGet]
        [Route("oauthDemo")]
        public string GetCodeAndState(string code, string state) {
            return code;
        }

        [HttpGet]
        [Route("oauthToken")]
        public async Task<string> GetToken(string code)
        {
            var values = new Dictionary<string, string>
            {
                {"code", code},
                {"client_id", "CBJCHBCAABAAO5iL7Ahz7onKLwPDXx6Prv0ZD07tz3Fo"},
                {"client_secret", "-RnP6Fc6igPX1gVOs8dj2znpdqAp75B-"},
                {"redirect_uri", "https://localhost:44386/oauthDemo"},
                {"grant_type", "authorization_code"}
            };

            var content = new FormUrlEncodedContent(values);
            var response = await _httpClient.PostAsync("https://api.na3.adobesign.com/oauth/v2/token", content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}