using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Processors.TweetsCleanUp
{
    public class SaveDeletedTweetStatusProcessor : ISaveDeletedTweetStatusProcessor
    {
        private readonly ISyncTweetsPostgresDal _syncTweetsPostgresDal;

        #region Ctor
        public SaveDeletedTweetStatusProcessor(ISyncTweetsPostgresDal syncTweetsPostgresDal)
        {
            _syncTweetsPostgresDal = syncTweetsPostgresDal;
        }
        #endregion

        public async Task ProcessAsync(TweetToDelete tweetToDelete, CancellationToken ct)
        {
            var highLimitDate = DateTime.UtcNow.AddDays(-40); //TODO get settings value
            if (tweetToDelete.DeleteSuccessful || tweetToDelete.Tweet.PublishedAt < highLimitDate)
            {
                await _syncTweetsPostgresDal.DeleteTweetAsync(tweetToDelete.Tweet.Id);
            }
        }
    }
}