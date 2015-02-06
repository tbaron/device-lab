using InfoSpace.DeviceLab.Status;

namespace InfoSpace.DeviceLab.Web
{
    public static class DeviceConfig
    {
        public static void Initialize()
        {
            DeviceManager.Instance.RestoreState();
        }

        public static void Shutdown()
        {
            DeviceManager.Instance.SaveState();
        }

        public static void AddLocalMockDevices()
        {
            for (var i = 0; i < 10; i++)
            {
                DeviceManager.Instance.AddDevice(new DeviceModel
                {
                    Id = "mock-device-" + i,
                    Name = "Local Mock Device",
                    Version = "1.1.1",
                    Status = "Online"
                });
            }
        }
    }
}
