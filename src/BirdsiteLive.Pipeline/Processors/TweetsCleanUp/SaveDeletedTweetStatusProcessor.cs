using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.TweetsCleanUp.Base;

namespace BirdsiteLive.Pipeline.Processors.TweetsCleanUp
{
    public class SaveDeletedTweetStatusProcessor : RetentionBase, ISaveDeletedTweetStatusProcessor
    {
        private readonly ISyncTweetsPostgresDal _syncTweetsPostgresDal;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public SaveDeletedTweetStatusProcessor(ISyncTweetsPostgresDal syncTweetsPostgresDal, InstanceSettings instanceSettings)
        {
            _syncTweetsPostgresDal = syncTweetsPostgresDal;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task ProcessAsync(TweetToDelete tweetToDelete, CancellationToken ct)
        {
            var retentionTime = GetRetentionTime(_instanceSettings);
            retentionTime += 20; // Delay until last retry
            var highLimitDate = DateTime.UtcNow.AddDays(-retentionTime);
            if (tweetToDelete.DeleteSuccessful || tweetToDelete.Tweet.PublishedAt < highLimitDate)
            {
                await _syncTweetsPostgresDal.DeleteTweetAsync(tweetToDelete.Tweet.Id);
            }
        }
    }
}