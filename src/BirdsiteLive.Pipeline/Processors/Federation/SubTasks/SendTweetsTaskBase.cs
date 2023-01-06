using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Pipeline.Processors.SubTasks
{
    public class SendTweetsTaskBase
    {
        private readonly ISyncTweetsPostgresDal _syncTweetsPostgresDal;

        #region Ctor
        protected SendTweetsTaskBase(ISyncTweetsPostgresDal syncTweetsPostgresDal)
        {
            _syncTweetsPostgresDal = syncTweetsPostgresDal;
        }
        #endregion

        protected async Task SaveSyncTweetAsync(string acct, long tweetId, string host, string inbox)
        {
            var inboxUrl = $"https://{host}/{inbox.Trim('/')}";
            var tweet = new SyncTweet
            {
                Acct = acct,
                TweetId = tweetId,
                PublishedAt = DateTime.UtcNow,
                Inbox = inboxUrl
            };
            await _syncTweetsPostgresDal.SaveTweetAsync(tweet);
        }
    }
}