using CommandLine;
using InfoSpace.DeviceLab.Service.Log;

namespace InfoSpace.DeviceLab.Service
{
    public class DeviceServiceConfig
    {
        [Option("log-level", DefaultValue = LogLevel.Info, Required = false)]
        public LogLevel LogLevel { get; set; }
    }
}
