using System.Linq;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Statistics.Domain;
using BirdsiteLive.Twitter.Extractors;
using BirdsiteLive.Twitter.Models;
using Tweetinvi;

namespace BirdsiteLive.Twitter
{
    public interface ITwitterUserService
    {
        TwitterUser GetUser(string username);
    }

    public class TwitterUserService : ITwitterUserService
    {
        private readonly TwitterSettings _settings;
        private readonly ITweetExtractor _tweetExtractor;
        private readonly ITwitterStatisticsHandler _statisticsHandler;

        #region Ctor
        public TwitterUserService(TwitterSettings settings, ITweetExtractor tweetExtractor, ITwitterStatisticsHandler statisticsHandler)
        {
            _settings = settings;
            _tweetExtractor = tweetExtractor;
            _statisticsHandler = statisticsHandler;
            Auth.SetApplicationOnlyCredentials(_settings.ConsumerKey, _settings.ConsumerSecret, true);
        }
        #endregion

        public TwitterUser GetUser(string username)
        {
            var user = User.GetUserFromScreenName(username);
            _statisticsHandler.CalledUserApi();
            if (user == null) return null;

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
                ProfileImageUrl = user.ProfileImageUrlFullSize,
                ProfileBackgroundImageUrl = user.ProfileBackgroundImageUrlHttps,
                ProfileBannerURL = user.ProfileBannerURL
            };
        }
    }
}