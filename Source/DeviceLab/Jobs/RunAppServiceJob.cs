using Newtonsoft.Json;

namespace InfoSpace.DeviceLab.Jobs
{
    public class RunAppServiceJob : ServiceJob
    {
        [JsonProperty("apkUrl")]
        public string ApkUrl { get; set; }

        public override bool Equals(object obj)
        {
            return
                base.Equals(obj) &&
                this.Equals(obj as RunAppServiceJob);
        }

        private bool Equals(RunAppServiceJob other)
        {
            return
                other != null &&
                other.ApkUrl == this.ApkUrl;
        }

        public override int GetHashCode()
        {
            return Hasher.Combine(
                base.GetHashCode(),
                Hasher.SafeHash(ApkUrl));
        }
    }
}
