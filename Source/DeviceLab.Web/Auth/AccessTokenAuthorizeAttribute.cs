using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace InfoSpace.DeviceLab.Web.Auth
{
    public class AccessTokenAuthorizeAttribute : AuthorizeAttribute
    {
        private BasicAuthenticator authenticator = new BasicAuthenticator();

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            return
                IsAuthenticatedUser(actionContext) ||
                HasValidAccessToken(actionContext);
        }

        private bool IsAuthenticatedUser(HttpActionContext actionContext)
        {
            return authenticator.IsAuthenticated(actionContext.Request);
        }

        private bool HasValidAccessToken(HttpActionContext actionContext)
        {
            string accessToken =
                GetTokenFromAuthorizationHeader(actionContext) ??
                GetTokenFromQueryString(actionContext);

            return KeyManager.Instance.IsValidAccessToken(accessToken);
        }

        private static string GetTokenFromAuthorizationHeader(HttpActionContext actionContext)
        {
            var auth = actionContext.Request.Headers.Authorization;

            if (auth != null &&
                StringComparer.OrdinalIgnoreCase.Equals("bearer", auth.Scheme))
            {
                return auth.Parameter;
            }

            return null;
        }

        private static string GetTokenFromQueryString(HttpActionContext actionContext)
        {
            var query = HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query);

            return query["access_token"];
        }
    }
}
