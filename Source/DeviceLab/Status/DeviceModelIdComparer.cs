using System.Collections.Generic;

namespace InfoSpace.DeviceLab.Status
{
    public class DeviceModelIdComparer : IEqualityComparer<DeviceModel>
    {
        public bool Equals(DeviceModel x, DeviceModel y)
        {
            return
                (x == null && y == null) ||
                (x != null && y != null && x.Id == y.Id);
        }

        public int GetHashCode(DeviceModel obj)
        {
            return obj != null ? obj.Id.GetHashCode() : 0;
        }
    }
}
