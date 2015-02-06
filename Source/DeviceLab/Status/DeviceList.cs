using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class DeviceList : IRefreshable
    {
        [JsonProperty("devices")]
        public DeviceModel[] Devices { get; set; }

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
