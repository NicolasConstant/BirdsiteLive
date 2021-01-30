using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using Microsoft.Extensions.Logging;
using Tweetinvi;

namespace BirdsiteLive.Twitter.Tools
{
    public interface ITwitterAuthenticationInitializer
    {
        void EnsureAuthenticationIsInitialized();
    }

    public class TwitterAuthenticationInitializer : ITwitterAuthenticationInitializer
    {
        private readonly TwitterSettings _settings;
        private readonly ILogger<TwitterAuthenticationInitializer> _logger;
        private static bool _initialized;
        private readonly SemaphoreSlim _semaphoregate = new SemaphoreSlim(1);

        #region Ctor
        public TwitterAuthenticationInitializer(TwitterSettings settings, ILogger<TwitterAuthenticationInitializer> logger)
        {
            _settings = settings;
            _logger = logger;
        }
        #endregion

        public void EnsureAuthenticationIsInitialized()
        {
            if (_initialized) return;
            _semaphoregate.Wait();
           
            try
            {
                if (_initialized) return;
                InitTwitterCredentials();
            }
            finally
            {
                _semaphoregate.Release();
            }
        }

        private void InitTwitterCredentials()
        {
            for (;;)
            {
                try
                {
                    Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
                    _initialized = true;
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Twitter Authentication Failed");
                    Thread.Sleep(250);
                }
            }
        }
    }
}