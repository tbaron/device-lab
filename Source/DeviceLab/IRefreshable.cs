using Newtonsoft.Json;

namespace InfoSpace.DeviceLab
{
    public interface IRefreshable
    {
        [JsonProperty("refreshToken")]
        string RefreshToken { get; }
    }
}
