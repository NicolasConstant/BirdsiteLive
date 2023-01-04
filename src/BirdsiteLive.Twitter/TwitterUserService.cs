using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Models;
using BirdsiteLive.Twitter.Tools;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterUserService
    {
        TwitterUser GetUser(string username);
    }

    public class TwitterUserService : ITwitterUserService
    {
        private readonly ITwitterAuthenticationInitializer _twitterAuthenticationInitializer;
        private readonly ITwitterStatisticsHandler _statisticsHandler;
        private readonly ILogger<TwitterUserService> _logger;
        private HttpClient _httpClient = new HttpClient();

        #region Ctor
        public TwitterUserService(ITwitterAuthenticationInitializer twitterAuthenticationInitializer, ITwitterStatisticsHandler statisticsHandler, ILogger<TwitterUserService> logger)
        {
            _twitterAuthenticationInitializer = twitterAuthenticationInitializer;
            _statisticsHandler = statisticsHandler;
            _logger = logger;
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            return GetUserAsync(username).Result;
        }
        public async Task<TwitterUser> GetUserAsync(string username)
        {
            //Proceed to account retrieval
            await _twitterAuthenticationInitializer.EnsureAuthenticationIsInitialized();

            JsonDocument res;
            try
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.twitter.com/2/users/by/username/"+ username + "?user.fields=name,username,protected,profile_image_url,url,description"))
    {
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _twitterAuthenticationInitializer.Token); 

                    var httpResponse = await _httpClient.SendAsync(request);
                    httpResponse.EnsureSuccessStatusCode();

                    var c = await httpResponse.Content.ReadAsStringAsync();
                    res = JsonDocument.Parse(c);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving user {Username}", username);
                throw;
            }
            finally
            {
                _statisticsHandler.CalledUserApi();
            }

            return new TwitterUser
            {
                Id = long.Parse(res.RootElement.GetProperty("data").GetProperty("id").GetString()),
                Acct = res.RootElement.GetProperty("data").GetProperty("username").GetString(),
                Name = res.RootElement.GetProperty("data").GetProperty("name").GetString(),
                Description = res.RootElement.GetProperty("data").GetProperty("description").GetString(),
                Url = res.RootElement.GetProperty("data").GetProperty("url").GetString(),
                ProfileImageUrl = res.RootElement.GetProperty("data").GetProperty("profile_image_url").GetString(),
                ProfileBackgroundImageUrl = res.RootElement.GetProperty("data").GetProperty("profile_image_url").GetString(), //for now
                ProfileBannerURL = res.RootElement.GetProperty("data").GetProperty("profile_image_url").GetString(), //for now
                Protected = res.RootElement.GetProperty("data").GetProperty("protected").GetBoolean(), 
            };
        }

        
    }
}