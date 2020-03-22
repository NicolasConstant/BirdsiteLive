using System;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterService
    {
        TwitterUser GetUser(string username);
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
            Auth.SetUserCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, _settings.AccessToken, _settings.AccessTokenSecret);
            var user = User.GetUserFromScreenName(username);

            return new TwitterUser
            {
                Name = user.Name,
                Description = user.Description,
                Url = user.Url,
                ProfileImageUrl = user.ProfileImageUrl,
                ProfileBackgroundImageUrl = user.ProfileBackgroundImageUrl
            };
        }
    }
}
