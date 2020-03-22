using System;
using BirdsiteLive.Twitter.Settings;
using Tweetinvi;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterService
    {
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

        public void GetUser(string username)
        {
            var user = User.GetUserFromScreenName(username);
        }
    }
}
