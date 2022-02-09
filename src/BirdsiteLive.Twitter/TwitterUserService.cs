using System;
using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Models;
using BirdsiteLive.Twitter.Tools;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterUserService
    {
        TwitterUser GetUser(string username);
        bool IsUserApiRateLimited();
    }

    public class TwitterUserService : ITwitterUserService
    {
        private readonly ITwitterAuthenticationInitializer _twitterAuthenticationInitializer;
        private readonly ITwitterStatisticsHandler _statisticsHandler;
        private readonly ILogger<TwitterUserService> _logger;

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
            //Check if API is saturated 
            if (IsUserApiRateLimited()) throw new RateLimitExceededException();

            //Proceed to account retrieval
            _twitterAuthenticationInitializer.EnsureAuthenticationIsInitialized();
            ExceptionHandler.SwallowWebExceptions = false;
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

            IUser user;
            try
            {
                user = User.GetUserFromScreenName(username);
            }
            catch (TwitterException e)
            {
                if (e.TwitterExceptionInfos.Any(x => x.Message.ToLowerInvariant().Contains("User has been suspended".ToLowerInvariant())))
                {
                    throw new UserHasBeenSuspendedException();
                }
                else if (e.TwitterExceptionInfos.Any(x => x.Message.ToLowerInvariant().Contains("User not found".ToLowerInvariant())))
                {
                    throw new UserNotFoundException();
                }
                else if (e.TwitterExceptionInfos.Any(x => x.Message.ToLowerInvariant().Contains("Rate limit exceeded".ToLowerInvariant())))
                {
                    throw new RateLimitExceededException();
                }
                else
                {
                    throw;
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

            // Expand URLs
            var description = user.Description;
            foreach (var descriptionUrl in user.Entities?.Description?.Urls?.OrderByDescending(x => x.URL.Length))
                description = description.Replace(descriptionUrl.URL, descriptionUrl.ExpandedURL);

            return new TwitterUser
            {
                Id = user.Id,
                Acct = username,
                Name = user.Name,
                Description = description,
                Url = $"https://twitter.com/{username}",
                ProfileImageUrl = user.ProfileImageUrlFullSize.Replace("http://", "https://"),
                ProfileBackgroundImageUrl = user.ProfileBackgroundImageUrlHttps,
                ProfileBannerURL = user.ProfileBannerURL,
                Protected = user.Protected
            };
        }

        public bool IsUserApiRateLimited()
        {
            // Retrieve limit from tooling
            _twitterAuthenticationInitializer.EnsureAuthenticationIsInitialized();
            ExceptionHandler.SwallowWebExceptions = false;
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

            try
            {
                var queryRateLimits = RateLimit.GetQueryRateLimit("https://api.twitter.com/1.1/users/show.json?screen_name=mastodon");

                if (queryRateLimits != null)
                {
                    return queryRateLimits.Remaining <= 0;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving rate limits");
            }

            // Fallback
            var currentCalls = _statisticsHandler.GetCurrentUserCalls();
            var maxCalls = _statisticsHandler.GetStatistics().UserCallsMax;
            return currentCalls >= maxCalls;
        }
    }
}