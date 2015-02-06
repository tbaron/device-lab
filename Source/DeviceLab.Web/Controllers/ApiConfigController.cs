using System;
using System.Web.Mvc;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    public class ApiConfigController : Controller
    {
        public ActionResult Index()
        {
            return View(KeyManager.Instance.GetKeys());
        }

        public ActionResult CreateKey(string applicationName)
        {
            KeyManager.Instance.AddKey(new KeyInfo
            {
                ApplicationName = applicationName,
                ClientId = KeyManager.Instance.GetRandomClientId(),
                ClientSecret = KeyManager.Instance.GetRandomClientSecret(),
                Date = DateTime.Now,
                CreatedBy = ""
            });

            return RedirectToAction("index");
        }

        public ActionResult RemoveKey(string clientId)
        {
            KeyManager.Instance.RemoveKey(clientId);

            return RedirectToAction("index");
        }

        public ActionResult RevokeToken(string accessToken)
        {
            KeyManager.Instance.RevokeAccessToken(accessToken);

            return RedirectToAction("index");
        }

        public ActionResult RemoveDevice(string deviceId)
        {
            DeviceManager.Instance.RemoveDevice(deviceId);

            return RedirectToAction("index");
        }
    }
}
