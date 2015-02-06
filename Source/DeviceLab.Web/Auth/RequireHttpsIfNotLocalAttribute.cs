using System;
using System.Web;
using System.Web.Mvc;

namespace InfoSpace.DeviceLab.Web.Auth
{
    public class RequireHttpsIfPublicAttribute : RequireHttpsAttribute
    {
        protected override void HandleNonHttpsRequest(AuthorizationContext filterContext)
        {
            if (!IsLocalRequest(filterContext.HttpContext.Request))
            {
                base.HandleNonHttpsRequest(filterContext);
            }
        }

        private bool IsLocalRequest(HttpRequestBase request)
        {
            return request.Url.Host == "127.0.0.1" || StringComparer.OrdinalIgnoreCase.Equals(request.Url.Host, "localhost");
        }
    }
}
