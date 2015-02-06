using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace InfoSpace.DeviceLab.Web.Auth
{
    public class BasicAuthenticator
    {
        private const string CookieName = "__Auth";
        private readonly string githubOrganization;
        private readonly Random random;

        public BasicAuthenticator()
        {
            githubOrganization = ConfigurationManager.AppSettings["auth.github.org"];
            random = new Random();
        }

        public void SetAuthenticated(HttpResponseBase response)
        {
            WriteAuthCookie(response);
        }

        private void WriteAuthCookie(HttpResponseBase response)
        {
            response.AppendCookie(CreateAuthCookie());
        }

        private HttpCookie CreateAuthCookie()
        {
            return new HttpCookie(CookieName, CreateAuthCookieValue());
        }

        private string CreateAuthCookieValue()
        {
            string value = githubOrganization + "/" + random.NextDouble();

            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(value));
        }

        public bool IsAuthenticated(HttpRequestBase request)
        {
            return IsLocalRequest(request) || HasAuthCookie(request);
        }

        private bool IsLocalRequest(HttpRequestBase request)
        {
            return request.Url.Host == "127.0.0.1" || StringComparer.OrdinalIgnoreCase.Equals(request.Url.Host, "localhost");
        }

        private bool HasAuthCookie(HttpRequestBase request)
        {
            var authCookie = request.Cookies[CookieName];

            return authCookie != null && IsValidAuthCookie(authCookie);
        }

        private bool IsValidAuthCookie(HttpCookie authCookie)
        {
            var value = Decrypt(authCookie.Value);

            return IsValidAuthCookieValue(value);
        }

        private static string Decrypt(string str)
        {
            try
            {
                return Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(str));
            }
            catch
            {
                return String.Empty;
            }
        }

        private bool IsValidAuthCookieValue(string value)
        {
            var values = value.Split('/');

            double salt;

            return values[0] == githubOrganization &&
                Double.TryParse(values[1], out salt);
        }

        public bool IsAuthenticated(HttpRequestMessage request)
        {
            return IsLocalRequest(request) || HasAuthCookie(request);
        }

        private bool IsLocalRequest(HttpRequestMessage request)
        {
            return request.RequestUri.Host == "127.0.0.1" || StringComparer.OrdinalIgnoreCase.Equals(request.RequestUri.Host, "localhost");
        }

        private bool HasAuthCookie(HttpRequestMessage request)
        {
            var cookies = request.Headers.GetCookies(CookieName);

            return cookies != null && cookies
                .SelectMany(x => x.Cookies)
                .Any(IsValidAuthCookie);
        }

        private bool IsValidAuthCookie(CookieState cookie)
        {
            return cookie != null &&
                IsValidAuthCookieValue(Decrypt(cookie.Value));
        }
    }
}
