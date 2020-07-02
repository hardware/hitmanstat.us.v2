using System;
using System.Net;
using Newtonsoft.Json;

namespace hitmanstat.us.Framework
{
    public class ReCaptcha
    {
        public static bool Validate(string token, IPAddress address)
        {
            using var client = new WebClient();

            var secret = Environment.GetEnvironmentVariable("RECAPTCHA_PRIVATE_KEY");
            var clientIp = address.ToString();

            var url = string.Format(
                "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}&remoteip={2}",
                secret, token, clientIp);

            var response = JsonConvert.DeserializeObject<ReCaptchaResponse>(client.DownloadString(url));

            if (response.Success && response.Action == "UserReport")
            {
                return (response.Score >= 0.5);
            }

            return false;
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
