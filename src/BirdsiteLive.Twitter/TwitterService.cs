using System;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;
using Tweetinvi.Models;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterService
    {
        TwitterUser GetUser(string username);
        ITweet GetTweet(long statusId);
    }

    public class TwitterService : ITwitterService
    {
        private readonly TwitterSettings _settings;

        #region Ctor
        public TwitterService(TwitterSettings settings)
        {
            _settings = settings;
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            //Auth.SetUserCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, _settings.AccessToken, _settings.AccessTokenSecret);
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
            var user = User.GetUserFromScreenName(username);
            if (user == null) return null;

            return new TwitterUser
            {
                Acct = username,
                Name = user.Name,
                Description = user.Description,
                Url = $"https://twitter.com/{username}",
                ProfileImageUrl = user.ProfileImageUrlFullSize,
                ProfileBackgroundImageUrl = user.ProfileBackgroundImageUrlHttps,
                ProfileBannerURL = user.ProfileBannerURL
            };
        }

        public ITweet GetTweet(long statusId)
        {
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
            var tweet = Tweet.GetTweet(statusId);
            return tweet;
        }
    }
}
