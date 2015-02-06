using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
using InfoSpace.DeviceLab.Service.Log;
using Newtonsoft.Json.Linq;

namespace InfoSpace.DeviceLab.Service
{
    public class ServiceAdapter
    {
        private readonly static string BaseServiceUrl = "https://" + ConfigurationManager.AppSettings["insp.service.hostname"];
        private readonly static string ClientId = ConfigurationManager.AppSettings["insp.service.clientid"];
        private readonly static string ClientSecret = ConfigurationManager.AppSettings["insp.service.clientsecret"];

        private string apiAccessToken;

        private SingleUseLock authLock = new SingleUseLock();
        private ManualResetEvent authHandle = new ManualResetEvent(true);

        static ServiceAdapter()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public ServiceAdapter()
        {
            AuthenticateIfTokenEqual(apiAccessToken);
        }

        public void Request(string path, Action<HttpWebRequest> handler)
        {
            WaitForAuthentication();

            var accessTokenAtTimeOfRequest = apiAccessToken;

            path = FullyQualifyUrl(path);
            Logger.Debug("Requesting URL: {0}", path);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);

            request.Headers["Authorization"] = "Bearer " + accessTokenAtTimeOfRequest;

            try
            {
                handler(request);
            }
            catch (WebException e)
            {
                HttpWebResponse response = e.Response as HttpWebResponse;

                if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    AuthenticateIfTokenEqual(accessTokenAtTimeOfRequest);
                }

                throw;
            }
        }

        private void AuthenticateIfTokenEqual(string token)
        {
            if (!authLock.TryEnter())
            {
                return;
            }

            if (token != apiAccessToken)
            {
                authLock.Reset();
                return;
            }

            authHandle.Reset();

            Logger.Info("Requesting new access token.");
            ThreadPool.QueueUserWorkItem(AuthenticateBackground);
        }

        private void AuthenticateBackground(object state)
        {
            try
            {
                apiAccessToken = GetAccessToken();
                Logger.Info("Got access token: " + apiAccessToken);
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get access token. {0}", e.Message);
            }
            finally
            {
                authHandle.Set();
                authLock.Reset();
            }
        }

        private string GetAccessToken()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(FullyQualifyUrl("api/oauth2/token"));

            request.Method = "POST";
            request.Headers["grant_type"] = "client_credentials";
            request.Accept = "application/json";
            request.ContentType = "application/x-www-form-urlencoded";

            string formData = String.Format("client_id={0}&client_secret={1}",
                Uri.EscapeDataString(ClientId),
                Uri.EscapeDataString(ClientSecret));

            Logger.Debug("Requesting access token for {0}", formData);

            using (var stream = request.GetRequestStream())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(formData);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var raw = reader.ReadToEnd();

                Logger.Debug("Request for access token returned: {0}" + raw);

                var json = JObject.Parse(raw);
                var token = json["access_token"];

                return token != null
                    ? token.Value<string>()
                    : null;
            }
        }

        private void WaitForAuthentication()
        {
            authHandle.WaitOne();
        }

        private static string FullyQualifyUrl(string path)
        {
            return BaseServiceUrl.TrimEnd('/') + "/" + path.TrimStart('/');
        }
    }
}
