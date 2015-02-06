using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;

namespace InfoSpace.DeviceLab.Web.Auth
{
    public class GitHubAuthorizationAttribute : AuthorizeAttribute
    {
        private readonly Random randomNumGenerator = new Random();
        private readonly BasicAuthenticator authenticator = new BasicAuthenticator();

        public string ClientId { get; set; }

        public string AfterAuthRedirectUrl { get; set; }

        public string[] Scopes { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return authenticator.IsAuthenticated(httpContext.Request);
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult(UnauthorizedRedirectUrl(filterContext));
        }

        private string UnauthorizedRedirectUrl(AuthorizationContext context)
        {
            return new UriBuilder("https://github.com/login/oauth/authorize")
            {
                Query = CreateQueryString(context.HttpContext.Request).ToString()
            }.Uri.ToString();
        }

        private NameValueCollection CreateQueryString(HttpRequestBase request)
        {
            var queryStringCollection = HttpUtility.ParseQueryString(String.Empty);

            queryStringCollection["client_id"] = ClientId;

            if (AfterAuthRedirectUrl != null)
            {
                queryStringCollection["redirect_uri"] = AfterAuthRedirectUrl + "?redirect=" + HttpUtility.UrlEncode(request.Url.ToString());
            }

            if (Scopes != null)
            {
                foreach (var scope in Scopes)
                {
                    queryStringCollection.Add("scope", scope);
                }
            }

            queryStringCollection["state"] = randomNumGenerator.NextDouble().ToString();

            return queryStringCollection;
        }
    }
}
