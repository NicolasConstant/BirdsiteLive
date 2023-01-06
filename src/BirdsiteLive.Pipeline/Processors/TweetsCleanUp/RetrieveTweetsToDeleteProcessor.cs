using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts.TweetsCleanUp;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.TweetsCleanUp.Base;

namespace BirdsiteLive.Pipeline.Processors.TweetsCleanUp
{
    public class RetrieveTweetsToDeleteProcessor : RetentionBase, IRetrieveTweetsToDeleteProcessor
    {
        private readonly ISyncTweetsPostgresDal _syncTweetsPostgresDal;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public RetrieveTweetsToDeleteProcessor(ISyncTweetsPostgresDal syncTweetsPostgresDal, InstanceSettings instanceSettings)
        {
            _syncTweetsPostgresDal = syncTweetsPostgresDal;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public async Task GetTweetsAsync(BufferBlock<TweetToDelete> tweetsBufferBlock, CancellationToken ct)
        {
            var batchSize = 100; 

            for (;;)
            {
                ct.ThrowIfCancellationRequested();

                var now = DateTime.UtcNow;

               
                var from = now.AddDays(-GetRetentionTime(_instanceSettings));
                var dbBrowsingEnded = false;
                var lastId = -1L;

                do
                {
                    var tweets = await _syncTweetsPostgresDal.GetTweetsOlderThanAsync(from, lastId, batchSize);

                    foreach (var syncTweet in tweets)
                    {
                        var tweet = new TweetToDelete
                        {
                            Tweet = syncTweet
                        };
                        await tweetsBufferBlock.SendAsync(tweet, ct);
                    }

                    if (tweets.Any()) lastId = tweets.Last().Id;
                    if (tweets.Count < batchSize) dbBrowsingEnded = true;

                } while (!dbBrowsingEnded);

                await Task.Delay(TimeSpan.FromHours(3), ct);
            }
        }
    }
}