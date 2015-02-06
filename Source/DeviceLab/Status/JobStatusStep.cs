using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class JobStatusStep
    {
        [JsonProperty("steps")]
        public ICollection<JobStatusStep> Steps { get; set; }

        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("errorLog")]
        public string ErrorLog { get; set; }

        [JsonProperty("outputLog")]
        public string OutputLog { get; set; }

        [JsonProperty("success")]
        public bool? Success { get; set; }
    }

    public class JobStatusStep<T> : JobStatusStep
    {
        [JsonProperty("result")]
        public T Result { get; set; }
    }
}
