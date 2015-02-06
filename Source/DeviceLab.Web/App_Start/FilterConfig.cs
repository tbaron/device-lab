using System.Configuration;
using System.Web.Mvc;
using InfoSpace.DeviceLab.Web.Auth;

namespace InfoSpace.DeviceLab.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            var clientId = ConfigurationManager.AppSettings["auth.github.clientid"];

            //filters.Add(new RequireHttpsIfPublicAttribute());
            filters.Add(new GitHubAuthorizationAttribute
            {
                ClientId = clientId
            });

            filters.Add(new HandleErrorAttribute());
        }
    }
}
