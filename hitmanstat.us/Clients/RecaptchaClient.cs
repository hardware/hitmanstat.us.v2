using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using hitmanstat.us.Framework;

namespace hitmanstat.us.Clients
{
    public class RecaptchaClient : IRecaptchaClient
    {
        private readonly HttpClient HttpClient;

        public RecaptchaClient(HttpClient httpClient) => HttpClient = httpClient;

        public async Task<bool> Validate(string token, IPAddress address)
        {
            var result = false;
            var secret = Environment.GetEnvironmentVariable("RECAPTCHA_PRIVATE_KEY");
            var clientIp = address.ToString();

            try
            {
                var response = await HttpClient
                    .PostAsync($"?secret={secret}&response={token}&remoteip={clientIp}", null);

                response.EnsureSuccessStatusCode();

                if (Utilities.IsJsonResponse(response.Content.Headers))
                {
                    var captcha = JsonConvert
                        .DeserializeObject<ReCaptchaResponse>(await response.Content.ReadAsStringAsync());

                    if (captcha.Success && captcha.Action == "UserReport")
                    {
                        result = (captcha.Score >= 0.5);
                    }
                }
            }
            catch
            { }

            return result;
        }

        private class ReCaptchaResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "score")]
            public float Score { get; set; }

            [JsonProperty(PropertyName = "action")]
            public string Action { get; set; }

            [JsonProperty(PropertyName = "challenge_ts")]
            public DateTime ChallengeTimestamp { get; set; }

            [JsonProperty(PropertyName = "hostname")]
            public string Hostname { get; set; }

            [JsonProperty(PropertyName = "error-codes")]
            public string[] Errors { get; set; }
        }
    }
}
