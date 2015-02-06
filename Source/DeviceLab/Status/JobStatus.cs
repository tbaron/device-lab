using System;
﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Status
{
    public class JobStatus : ServiceStatus
    {
        [JsonProperty("jobId")]
        public Guid JobId { get; set; }

        [JsonProperty("steps")]
        public ICollection<JobStatusStep> Steps { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
