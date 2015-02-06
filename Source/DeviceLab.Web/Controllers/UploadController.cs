using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    public class UploadController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [ActionName("apk"), HttpGet]
        [AllowAnonymous]
        public ActionResult GetApk(string name)
        {
            string path = Path.Combine(HostingEnvironment.MapPath("~/App_Data/apks"), name);
            FileInfo info = new FileInfo(path);

            if (!info.Exists)
            {
                return HttpNotFound();
            }

            return File(info.FullName, "application/octet-stream", info.Name);
        }
    }
}
