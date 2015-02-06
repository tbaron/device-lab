using System.IO;

namespace InfoSpace.DeviceLab
{
    public class PackageInfo
    {
        public string Id { get; set; }

        public string LaunchableActivity { get; set; }

        public int Version { get; set; }

        public string VersionName { get; set; }

        public string Label { get; set; }

        public Stream Icon { get; set; }
    }
}
