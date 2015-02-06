using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class DeviceModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("jobStatus")]
        public JobStatus JobStatus { get; set; }
    }
}
