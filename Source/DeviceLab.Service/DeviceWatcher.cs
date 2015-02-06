using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfoSpace.DeviceLab.Service.Log;
using InfoSpace.DeviceLab.Status;

namespace InfoSpace.DeviceLab.Service
{
    public class DeviceWatcher : IDisposable, IDeviceList
    {
        public delegate void OnAttachmentHandler(DeviceId deviceInfo);

        public readonly static TimeSpan DefaultInterval = TimeSpan.FromSeconds(10);

        private readonly object startLock = new object();
        private volatile bool isCancelled;
        private volatile bool isStarted;

        public event OnAttachmentHandler onAttach;
        public event OnAttachmentHandler onDetach;

        public DeviceWatcher()
        {
            Devices = new List<DeviceId>();
            Interval = DefaultInterval;
        }

        public ICollection<DeviceId> Devices
        {
            get;
            private set;
        }
        public TimeSpan Interval
        {
            get;
            set;
        }

        public void Watch()
        {
            if (!isStarted)
            {
                lock (startLock)
                {
                    if (!isStarted)
                    {
                        isStarted = true;
                        new Thread(BackgroundWorker).Start();
                    }
                }
            }
        }

        private void BackgroundWorker(object state)
        {
            while (!isCancelled)
            {
                try
                {
                    RefreshDeviceList().Wait();
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                }

                Thread.Sleep(Interval);
            }
        }

        private async Task RefreshDeviceList()
        {
            var currentDevices = new List<DeviceId>(Devices);

            var status = await new DeviceHelper().GetDevices();
            string[] deviceIds = status.Result;

            var detatchedDevices = currentDevices
                .Where(d => !deviceIds.Contains(d.HardwareId))
                .ToArray();

            foreach (var device in detatchedDevices)
            {
                currentDevices.Remove(device);
                if (onDetach != null)
                {
                    onDetach(device);
                }
            }

            var attachedDeviceIds = deviceIds
                .Where(id => !currentDevices.Any(d => d.HardwareId == id))
                .ToArray();
            ;

            foreach (var id in attachedDeviceIds)
            {
                var device = new DeviceId(id);

                currentDevices.Add(device);

                var task = PopulateDevice(device).ContinueWith(t =>
                {
                    if (t.Result != null && t.Result.Success == true && onAttach != null)
                    {
                        onAttach(device);
                    }
                });
            }

            Devices = currentDevices;
        }

        private async Task<JobStatusStep> PopulateDevice(DeviceId device)
        {
            return await new DeviceHelper().PopulateDeviceData(device);
        }

        public void Dispose()
        {
            isCancelled = true;
        }
    }
}
