using System.Collections.Generic;

namespace InfoSpace.DeviceLab.Service
{
    public interface IDeviceList
    {
        ICollection<DeviceId> Devices { get; }
    }
}
