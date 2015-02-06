using CommandLine;
using InfoSpace.DeviceLab.Service.Log;

namespace InfoSpace.DeviceLab.Service
{
    static class Program
    {
        static void Main(string[] args)
        {
            DeviceServiceConfig config = GetConfig(args);

            RunInProcess(config);
        }

        private static DeviceServiceConfig GetConfig(string[] args)
        {
            DeviceServiceConfig config = new DeviceServiceConfig();

            var result = Parser.Default.ParseArguments(args, config);

            return config;
        }

        private static void RunInProcess(DeviceServiceConfig config)
        {
            Logger.SetLogLevel(config.LogLevel);

            new DeviceService().Start().Wait();
        }
    }
}
