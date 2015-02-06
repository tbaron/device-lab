using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using InfoSpace.DeviceLab.Jobs;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    public class DeviceController : Controller
    {
        public ActionResult Index()
        {
            AddLocalMockDevicesOnceIfLocalRequest();

            return View();
        }

        public ActionResult apk(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
            }

            return View();
        }

        #region Local Mock Testing

        private static readonly SingleUseLock dummyDeviceLock = new SingleUseLock();

        private void AddLocalMockDevicesOnceIfLocalRequest()
        {
            if (dummyDeviceLock.TryEnter() &&
                IsLocalRequest())
            {
                DeviceConfig.AddLocalMockDevices();
            }
        }

        private bool IsLocalRequest()
        {
            return Request.Url.Host == "127.0.0.1" || StringComparer.OrdinalIgnoreCase.Equals(Request.Url.Host, "localhost");
        }

        #endregion
    }
}
