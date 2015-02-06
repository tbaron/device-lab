using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using InfoSpace.DeviceLab.Jobs;
using InfoSpace.DeviceLab.Status;
using InfoSpace.DeviceLab.Web.Auth;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    [AccessTokenAuthorize]
    public class ServiceController : ApiController
    {
        private readonly DeviceManager deviceManager;
        private readonly JobManager jobManager;

        public ServiceController()
        {
            deviceManager = DeviceManager.Instance;
            jobManager = JobManager.Instance;
        }

        [HttpGet, ActionName("job"), ResponseType(typeof(ServiceJob))]
        public async Task<HttpResponseMessage> GetJob(string refreshToken = null, int timeout = 0)
        {
            var refreshManager = jobManager.RefreshManager;

            bool isRefreshed = await refreshManager.WaitForNextRefreshAsync(refreshToken, TimeSpan.FromSeconds(timeout));

            if (!isRefreshed)
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            return Request.CreateResponse(new ServiceJobRefresh
            {
                Job = jobManager.CurrentJob,
                RefreshToken = refreshManager.CurrentToken
            });
        }

        [HttpPost, ActionName("status")]
        public void PostStatus([FromBody]ServiceStatus[] messages)
        {
            StoreInDatabase(messages);

            HandleMessages(messages);
        }

        private static void StoreInDatabase(ServiceStatus[] messages)
        {
            var batchSize = DatabaseConfig.DocumentStore.Conventions.MaxNumberOfRequestsPerSession;

            foreach (var batch in messages.Batch(batchSize))
            {
                using (var session = DatabaseConfig.DocumentStore.OpenSession())
                {
                    foreach (var message in batch)
                    {
                        session.Store(message);
                    }

                    session.SaveChanges();
                }
            }
        }

        private void HandleMessages(ServiceStatus[] messages)
        {
            foreach (var message in messages)
            {
                HandleMessage(message);
            }

            deviceManager.SaveState();
        }

        private void HandleMessage(ServiceStatus message)
        {
            if (message is DeviceAttachServiceStatus)
            {
                HandleDeviceAttach(message as DeviceAttachServiceStatus);
            }
            else if (message is JobStatus)
            {
                HandleJobStatus(message as JobStatus);
            }
        }

        private void HandleDeviceAttach(DeviceAttachServiceStatus status)
        {
            if (status.Device == null)
            {
                return;
            }

            if (status.IsAttached)
            {
                deviceManager.AddDevice(status.Device);
            }
            else
            {
                deviceManager.RemoveDevice(status.Device);
            }
        }

        private void HandleJobStatus(JobStatus jobStatus)
        {
            deviceManager.UpdateStatus(jobStatus);
        }
    }
}
