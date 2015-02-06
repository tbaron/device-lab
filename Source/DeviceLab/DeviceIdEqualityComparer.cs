using System.Collections.Generic;

namespace InfoSpace.DeviceLab
{
    public class DeviceIdEqualityComparer : IEqualityComparer<DeviceId>
    {
        public bool Equals(DeviceId x, DeviceId y)
        {
            return x == null && y == null ||
                (x != null && y != null && x.HardwareId == y.HardwareId);
        }

        public int GetHashCode(DeviceId obj)
        {
            return obj == null ? 0
                : obj.HardwareId == null ? 0
                : obj.HardwareId.GetHashCode();
        }
    }
}
