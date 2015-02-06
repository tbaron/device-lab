using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using InfoSpace.DeviceLab.Jobs;
using InfoSpace.DeviceLab.Status;
using InfoSpace.DeviceLab.Web.Auth;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    [AccessTokenAuthorize]
    public class DeviceApiController : ApiController
    {
        private readonly JobManager jobManager;

        public DeviceApiController()
        {
            jobManager = JobManager.Instance;
        }

        [HttpGet, ActionName("list"), ResponseType(typeof(DeviceList))]
        public async Task<HttpResponseMessage> GetList(string refreshToken = null, int timeout = 0)
        {
            DeviceManager manager = DeviceManager.Instance;
            bool isRefreshed = await manager.RefreshManager.WaitForNextRefreshAsync(refreshToken, TimeSpan.FromSeconds(timeout));

            if (!isRefreshed)
            {
                return new HttpResponseMessage(HttpStatusCode.NotModified);
            }

            return Request.CreateResponse(new DeviceList
            {
                Devices = manager.GetDevices(),
                RefreshToken = manager.RefreshManager.CurrentToken
            });
        }

        [HttpPost, ActionName("apk")]
        public void PostApk(FormDataCollection formData)
        {
            string url = EnsureUrlScheme(formData.Get("apkUrl"));

            jobManager.SetCurrentJob(new RunAppServiceJob
            {
                ApkUrl = url
            });
        }

        [HttpPost, ActionName("url")]
        public void PostUrl(FormDataCollection formData)
        {
            string url = EnsureUrlScheme(formData.Get("url"));

            jobManager.SetCurrentJob(new RunUrlServiceJob
            {
                Url = url
            });
        }

        [HttpPost, ActionName("stresstest")]
        public void PostStressTest()
        {
            jobManager.SetCurrentJob(new RunStressTestServiceJob());
        }

        private static string EnsureUrlScheme(string url)
        {
            if (url.IndexOf("://") == -1)
            {
                url = "http://" + url;
            }

            return url;
        }
    }
}
