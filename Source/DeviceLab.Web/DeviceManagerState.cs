using InfoSpace.DeviceLab.Status;

namespace InfoSpace.DeviceLab.Web
{
    public class DeviceManagerState
    {
        public const string DefaultId = "device-manager-state";

        public DeviceManagerState()
        {
            Id = DefaultId;
        }

        public string Id { get; set; }

        public DeviceModel[] Devices { get; set; }
    }
}
