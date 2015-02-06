using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class DeviceLogServiceStatus : ServiceStatus
    {
        [JsonProperty("packageId")]
        public string PackageId { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("lines")]
        public string[] Lines { get; set; }
    }
}
