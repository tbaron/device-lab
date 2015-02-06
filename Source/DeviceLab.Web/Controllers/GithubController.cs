using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using InfoSpace.DeviceLab.Web.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace InfoSpace.DeviceLab.Web.Controllers
{
    public class GithubController : Controller
    {
        private readonly string githubClientId;
        private readonly string githubClientSecret;
        private readonly string githubOrganization;
        private readonly string githubAccessToken;

        private readonly static MediaTypeFormatter[] formatters = 
        {
            new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    ContractResolver = new RubyPropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }
            }
        };

        private readonly HttpClient client;

        public GithubController()
        {
            githubOrganization = ConfigurationManager.AppSettings["auth.github.org"];
            githubAccessToken = ConfigurationManager.AppSettings["auth.github.accesstoken"];
            githubClientId = ConfigurationManager.AppSettings["auth.github.clientid"];
            githubClientSecret = ConfigurationManager.AppSettings["auth.github.clientsecret"];

            client = CreateHttpClient();
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaTypeFormatter.DefaultMediaType.MediaType));

            client.DefaultRequestHeaders.AcceptEncoding.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("device-lab", GetType().Assembly.GetName().Version.ToString()));

            return client;
        }

        [AllowAnonymous]
        public ActionResult Callback(string code, string state)
        {
            if (!IsValidState(state))
            {
                throw new HttpException(400, "Invalid 'state' argument");
            }

            GitHubTokenResponse token = GetAccessToken(code);

            if (!IsTokenValid(token))
            {
                throw new HttpException(400, "Failed to exchange 'code' for access token");
            }

            GitHubUserResponse userProfile = GetUserProfile(token);

            if (!IsUserProfileValid(userProfile))
            {
                throw new HttpException(500, "Failed to retrieve user profile");
            }

            if (!IsUserInOrg(userProfile.Login))
            {
                throw new HttpException(403, "Not authorized");
            }

            new BasicAuthenticator().SetAuthenticated(Response);

            return RedirectToAction("index", "device");
        }

        private bool IsValidState(string state)
        {
            // TODO: implement state validation to prevent forgery
            // HACK: temporarily accept all states
            return true;
        }

        private GitHubTokenResponse GetAccessToken(string code)
        {
            using (var response = client.PostAsJsonAsync("https://github.com/login/oauth/access_token", new
            {
                client_id = githubClientId,
                client_secret = githubClientSecret,
                code = code
            }).Result)
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return response
                    .Content
                    .ReadAsAsync<GitHubTokenResponse>(formatters)
                    .Result;
            }
        }

        private static bool IsTokenValid(GitHubTokenResponse token)
        {
            return token != null &&
                !String.IsNullOrWhiteSpace(token.AccessToken);
        }

        private GitHubUserResponse GetUserProfile(GitHubTokenResponse token)
        {
            var httpResponse = client
                .GetAsync(GetUserProfileUrl(token))
                .Result;

            if (!httpResponse.IsSuccessStatusCode)
            {
                return null;
            }

            return httpResponse
                .Content
                .ReadAsAsync<GitHubUserResponse>(formatters)
                .Result;
        }

        private static string GetUserProfileUrl(GitHubTokenResponse token)
        {
            return String.Format("https://api.github.com/user?access_token={0}",
                token.AccessToken);
        }

        private static bool IsUserProfileValid(GitHubUserResponse userProfile)
        {
            return userProfile != null &&
                !String.IsNullOrWhiteSpace(userProfile.Login);
        }

        private bool IsUserInOrg(string username)
        {
            var httpResponse = client
                   .GetAsync(GetMembershipCheckUrl(username))
                   .Result;

            return httpResponse.IsSuccessStatusCode;
        }

        private string GetMembershipCheckUrl(string username)
        {
            return String.Format("https://api.github.com/orgs/{0}/members/{1}?access_token={2}",
                githubOrganization,
                username,
                githubAccessToken);
        }
    }

    public class GitHubTokenResponse
    {
        public string AccessToken { get; set; }
    }

    public class GitHubUserResponse
    {
        public string Login { get; set; }
        public string AvatarUrl { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
    }

    public class RubyPropertyNamesContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return Regex.Replace(propertyName, @"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", "$1$3_$2$4").ToLower();
        }
    }
}
