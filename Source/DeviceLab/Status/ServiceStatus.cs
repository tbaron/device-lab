using System;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class ServiceStatus
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }
}
