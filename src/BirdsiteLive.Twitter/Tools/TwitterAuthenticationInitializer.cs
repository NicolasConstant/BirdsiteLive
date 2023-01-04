using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using BirdsiteLive.Common.Settings;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BirdsiteLive.Twitter.Tools
{
    public interface ITwitterAuthenticationInitializer
    {
        Task EnsureAuthenticationIsInitialized();
        String Token {get;}
    }

    public class TwitterAuthenticationInitializer : ITwitterAuthenticationInitializer
    {
        private readonly TwitterSettings _settings;
        private readonly ILogger<TwitterAuthenticationInitializer> _logger;
        private static bool _initialized;
        private readonly SemaphoreSlim _semaphoregate = new SemaphoreSlim(1);
        private readonly IHttpClientFactory _httpClientFactory;
        private String _token;
        public String Token { 
            get { return _token; }
        }


        #region Ctor
        public TwitterAuthenticationInitializer(TwitterSettings settings, ILogger<TwitterAuthenticationInitializer> logger, IHttpClientFactory httpClientFactory)
        {
            _settings = settings;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        #endregion

        public async Task EnsureAuthenticationIsInitialized()
        {
            if (_initialized) return;
            _semaphoregate.Wait();
           
            try
            {
                if (_initialized) return;
                await InitTwitterCredentials();
            }
            finally
            {
                _semaphoregate.Release();
            }
        }

        private async Task InitTwitterCredentials()
        {
            for (;;)
            {
                try
                {
                    Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);

                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.twitter.com/oauth2/token"))
                    {
                        var httpClient = _httpClientFactory.CreateClient();
                        var base64authorization = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(_settings.ConsumerKey + ":" + _settings.ConsumerSecret));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}"); 

                        request.Content = new StringContent("grant_type=client_credentials");
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded"); 

                        var httpResponse = await httpClient.SendAsync(request);

                        var c = await httpResponse.Content.ReadAsStringAsync();
                        httpResponse.EnsureSuccessStatusCode();
                        var doc = JsonDocument.Parse(c);
                        _token = doc.RootElement.GetProperty("access_token").GetString();
                    }

                    _initialized = true;
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Twitter Authentication Failed");
                    await Task.Delay(250);
                }
            }
        }
    }
}