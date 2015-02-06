using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Jobs
{
    public class RunUrlServiceJob : ServiceJob
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                this.Equals(obj as RunUrlServiceJob);
        }

        private bool Equals(RunUrlServiceJob other)
        {
            return
                other != null &&
                other.Url == this.Url;
        }

        public override int GetHashCode()
        {
            return Hasher.Combine(
                base.GetHashCode(),
                Hasher.SafeHash(Url));
        }
    }
}
