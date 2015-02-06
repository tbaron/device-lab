using System;
using System.Linq;
using System.Security.Cryptography;
using Raven.Client;

namespace InfoSpace.DeviceLab.Web
{
    public class KeyManager
    {
        private static readonly Lazy<KeyManager> instance = new Lazy<KeyManager>();

        public static KeyManager Instance
        {
            get { return instance.Value; }
        }

        private readonly RandomNumberGenerator randomGenerator = new RNGCryptoServiceProvider();

        public KeyManager()
        {
        }

        public void AddKey(KeyInfo key)
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                session.Store(key);
                session.SaveChanges();
            }
        }

        public string GetRandomClientId()
        {
            return GetRandomData(16);
        }

        public string GetRandomClientSecret()
        {
            return GetRandomData(32);
        }

        private string GetRandomAccessToken()
        {
            return GetRandomData(32);
        }

        private string GetRandomData(int length)
        {
            byte[] buffer = new byte[length];

            randomGenerator.GetBytes(buffer);

            return Base62Encoder.ToBase62(buffer);
        }

        public KeyInfo[] GetKeys()
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                return session
                    .Query<KeyInfo>()
                    .OrderByDescending(x => x.Date)
                    .ToArray();
            }
        }

        public void RemoveKey(string clientId)
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                foreach (var key in session
                    .Query<KeyInfo>()
                    .Where(x => x.ClientId == clientId))
                {
                    session.Delete(key);
                }

                session.SaveChanges();
            }
        }

        public void RevokeAccessToken(string accessToken)
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                var keys = session
                    .Query<KeyInfo>()
                    .Where(key => key.AccessTokens.Any(x => x == accessToken));

                foreach (var key in keys)
                {
                    key.AccessTokens.Remove(accessToken);
                }

                session.SaveChanges();
            }
        }

        public string CreateAccessToken(string clientId, string clientSecret)
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                var keyInfo = session
                    .Query<KeyInfo>()
                    .Where(x => x.ClientId == clientId && x.ClientSecret == clientSecret)
                    .FirstOrDefault();

                if (keyInfo == null)
                {
                    return null;
                }

                var token = keyInfo.AccessTokens.LastOrDefault();

                if (token == null)
                {
                    token = GetRandomAccessToken();
                    keyInfo.AccessTokens.Add(token);
                    session.Store(keyInfo);
                    session.SaveChanges();
                }

                return token;
            }
        }

        public bool IsValidAccessToken(string accessToken)
        {
            using (var session = DatabaseConfig.DocumentStore.OpenSession())
            {
                return session
                    .Query<KeyInfo>()
                    .Any(keyInfo => keyInfo.AccessTokens.Any(x => x == accessToken));
            }
        }
    }
}
