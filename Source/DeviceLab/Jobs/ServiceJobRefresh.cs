using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Jobs
{
    public class ServiceJobRefresh : IRefreshable
    {
        [JsonProperty("job")]
        public ServiceJob Job { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
