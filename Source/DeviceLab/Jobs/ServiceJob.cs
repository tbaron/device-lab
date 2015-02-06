using System;
using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Jobs
{
    public class ServiceJob
    {
        public ServiceJob()
        {
            Id = Guid.NewGuid();
            Time = DateTime.UtcNow;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as ServiceJob);
        }

        private bool Equals(ServiceJob other)
        {
            return other != null &&
                other.Id == this.Id &&
                other.Time == this.Time;
        }

        public override int GetHashCode()
        {
            return Hasher.Combine(
                Hasher.Base,
                Hasher.SafeHash(Id),
                Time.GetHashCode());
        }
    }
}
