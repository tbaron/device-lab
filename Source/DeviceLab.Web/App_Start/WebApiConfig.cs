using System.Web.Http;

namespace InfoSpace.DeviceLab.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "UploadApk",
                routeTemplate: "api/upload",
                defaults: new { Controller = "UploadApi" }
            );

            config.Routes.MapHttpRoute(
                name: "DeviceApi",
                routeTemplate: "api/device/{action}",
                defaults: new { Controller = "DeviceApi" }
            );

            config.Routes.MapHttpRoute(
                name: "AccessToken",
                routeTemplate: "api/oauth2/token",
                defaults: new { Controller = "AccessTokenApi" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}