using System;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class DeviceAttachServiceStatus : ServiceStatus
    {
        [JsonProperty("isAttached")]
        public bool IsAttached { get; set; }

        [JsonProperty("device")]
        public DeviceId Device { get; set; }
    }
}
