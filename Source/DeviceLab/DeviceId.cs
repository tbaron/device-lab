using Newtonsoft.Json;

namespace InfoSpace.DeviceLab
{
    public class DeviceId
    {
        public DeviceId()
        {
        }

        public DeviceId(string deviceId)
        {
            HardwareId = deviceId;
        }

        [JsonProperty("hardwareId")]
        public string HardwareId { get; set; }

        [JsonProperty("buildVersion")]
        public string BuildVersion { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }
    }
}
