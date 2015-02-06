using System.Diagnostics;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InfoSpace.DeviceLab.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly string Copyright = ReadCopyright();

        private static string ReadCopyright()
        {
            var info = FileVersionInfo.GetVersionInfo(typeof(MvcApplication).Assembly.Location);

            return info.LegalCopyright;
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            SerializerConfig.RegisterSerializers(GlobalConfiguration.Configuration.Formatters);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DatabaseConfig.InitializeDatabase();
            DeviceConfig.Initialize();
        }

        protected void Application_End()
        {
            DeviceConfig.Shutdown();
            DatabaseConfig.Shutdown();
        }
    }
}