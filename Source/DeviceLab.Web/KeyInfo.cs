using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Web
{
    public class KeyInfo
    {
        public KeyInfo()
        {
            AccessTokens = new List<string>();
        }

        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("applicationName")]
        public string ApplicationName { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("accessTokens")]
        public ICollection<string> AccessTokens { get; private set; }
    }
}
