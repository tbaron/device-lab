using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace InfoSpace.DeviceLab.Web
{
    public class RefreshTokenManager
    {
        private readonly static TimeSpan MinimumTimeout = TimeSpan.Zero;
        private readonly static TimeSpan MaximumTimeout = TimeSpan.FromSeconds(60);

        private readonly ConcurrentDictionary<string, ManualResetEvent> refreshHandles = new ConcurrentDictionary<string, ManualResetEvent>();

        public RefreshTokenManager()
        {
            SetNewToken();
        }

        public string CurrentToken
        {
            get;
            private set;
        }

        public void SetNewToken()
        {
            SetNewToken(Guid.NewGuid().ToString("N"));
        }

        public void SetNewToken(string newToken)
        {
            string token = this.CurrentToken ?? "";
            this.CurrentToken = newToken;

            ManualResetEvent handle;
            if (token != null && refreshHandles.TryGetValue(token, out handle))
            {
                using (handle)
                {
                    handle.Set();
                }
            }
        }

        public async Task<bool> WaitForNextRefreshAsync(string refreshToken, TimeSpan timeout)
        {
            return await Task.Factory.StartNew(() =>
            {
                return WaitForNextRefresh(refreshToken, timeout);
            });
        }

        public bool WaitForNextRefresh(string refreshToken, TimeSpan timeout)
        {
            if (refreshToken != CurrentToken)
            {
                return true;
            }

            if (timeout < MinimumTimeout)
            {
                timeout = MinimumTimeout;
            }
            if (timeout > MaximumTimeout)
            {
                timeout = MaximumTimeout;
            }

            WaitHandle handle = GetWaitHandle(refreshToken);

            handle.WaitOne(timeout);

            return refreshToken != CurrentToken;
        }

        private WaitHandle GetWaitHandle(string refreshToken)
        {
            return refreshHandles.GetOrAdd(refreshToken ?? "", _ => new ManualResetEvent(false));
        }
    }
}
