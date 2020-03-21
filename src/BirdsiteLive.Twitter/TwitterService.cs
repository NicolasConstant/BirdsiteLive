using System;
using BirdsiteLive.Twitter.Settings;

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
    }
}
