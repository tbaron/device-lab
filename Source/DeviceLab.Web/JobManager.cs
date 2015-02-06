using System;
using InfoSpace.DeviceLab.Jobs;

namespace InfoSpace.DeviceLab.Web
{
    public class JobManager
    {
        private readonly static Lazy<JobManager> instance = new Lazy<JobManager>(() => new JobManager(DeviceManager.Instance));
        private DeviceManager deviceManager;

        public static JobManager Instance
        {
            get { return instance.Value; }
        }

        public JobManager(DeviceManager deviceManager)
        {
            if (deviceManager == null)
            {
                throw new ArgumentNullException("deviceManager");
            }

            this.deviceManager = deviceManager;
            this.RefreshManager = new RefreshTokenManager();
        }

        public ServiceJob CurrentJob
        {
            get;
            private set;
        }

        public RefreshTokenManager RefreshManager
        {
            get;
            private set;
        }

        public void SetCurrentJob(ServiceJob job)
        {
            CurrentJob = job;

            deviceManager.ClearJobStatus();

            if (job == null)
            {
                RefreshManager.SetNewToken();
            }
            else
            {
                RefreshManager.SetNewToken(job.Id.ToString());
            }
        }
    }
}
