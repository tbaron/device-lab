using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using InfoSpace.DeviceLab.Status;

namespace InfoSpace.DeviceLab.Web
{
    public class DeviceManager
    {
        private readonly static Lazy<DeviceManager> instance = new Lazy<DeviceManager>(() => new DeviceManager());

        public static DeviceManager Instance
        {
            get { return instance.Value; }
        }

        private readonly ISet<DeviceModel> devices;

        public DeviceManager()
        {
            devices = new HashSet<DeviceModel>(new DeviceModelIdComparer());
            RefreshManager = new RefreshTokenManager();
        }

        public RefreshTokenManager RefreshManager
        {
            get;
            private set;
        }

        public DeviceModel[] GetDevices()
        {
            return devices.ToArray();
        }

        public void SaveState()
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                var state = session.Load<DeviceManagerState>(DeviceManagerState.DefaultId);

                if (state == null)
                {
                    state = new DeviceManagerState();
                    session.Store(state);
                }

                state.Devices = this.devices.ToArray();

                session.SaveChanges();
            }
        }

        public void RestoreState()
        {
            var state = GetSavedState();

            if (state != null)
            {
                foreach (var device in state.Devices)
                {
                    this.devices.Add(device);
                }
            }

            RefreshManager.SetNewToken();
        }

        private DeviceManagerState GetSavedState()
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                return session.Load<DeviceManagerState>(DeviceManagerState.DefaultId);
            }
        }

        public void AddDevice(DeviceId device)
        {
            var newDevice = ConvertDeviceToViewModel(device);

            AddDevice(newDevice);
        }

        public void AddDevice(DeviceModel device)
        {
            devices.Remove(device);
            devices.Add(device);

            RefreshManager.SetNewToken();
        }

        public void RemoveDevice(DeviceId device)
        {
            RemoveDevice(device.HardwareId);
        }

        public void RemoveDevice(string hardwareId)
        {
            devices.Remove(FindDeviceForId(hardwareId));

            RefreshManager.SetNewToken();
        }

        public void UpdateStatus(JobStatus jobStatus)
        {
            var device = FindDeviceForId(jobStatus.DeviceId);

            device.JobStatus = jobStatus;
            device.Status = jobStatus.Success ? "Online" : "Error";

            RefreshManager.SetNewToken();
        }

        private DeviceModel FindDeviceForId(string deviceId)
        {
            return devices.First(x => x.Id == deviceId);
        }

        private DeviceModel ConvertDeviceToViewModel(DeviceId device)
        {
            return new DeviceModel
            {
                Id = device.HardwareId,
                Name = Capitalize(device.Brand + " " + device.Model),
                Status = "Online",
                Type = "", // smartphone / tablet , etc
                Version = "Android " + device.BuildVersion
            };
        }

        private static string Capitalize(string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }

        public void ClearJobStatus()
        {
            foreach (var device in devices)
            {
                device.JobStatus = null;
                device.Status = "Pending";
            }

            SaveState();

            RefreshManager.SetNewToken();
        }
    }
}
