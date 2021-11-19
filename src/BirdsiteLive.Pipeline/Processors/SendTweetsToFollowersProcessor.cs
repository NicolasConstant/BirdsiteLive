using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Domain;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;
using BirdsiteLive.Pipeline.Processors.SubTasks;
using BirdsiteLive.Twitter;
using BirdsiteLive.Twitter.Models;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SendTweetsToFollowersProcessor : ISendTweetsToFollowersProcessor
    {
        private readonly ISendTweetsToInboxTask _sendTweetsToInboxTask;
        private readonly ISendTweetsToSharedInboxTask _sendTweetsToSharedInbox;
        private readonly IFollowersDal _followersDal;
        private readonly ILogger<SendTweetsToFollowersProcessor> _logger;

        #region Ctor
        public SendTweetsToFollowersProcessor(ISendTweetsToInboxTask sendTweetsToInboxTask, ISendTweetsToSharedInboxTask sendTweetsToSharedInbox, IFollowersDal followersDal, ILogger<SendTweetsToFollowersProcessor> logger)
        {
            _sendTweetsToInboxTask = sendTweetsToInboxTask;
            _sendTweetsToSharedInbox = sendTweetsToSharedInbox;
            _logger = logger;
            _followersDal = followersDal;
        }
        #endregion

        public async Task<UserWithDataToSync> ProcessAsync(UserWithDataToSync userWithTweetsToSync, CancellationToken ct)
        {
            var user = userWithTweetsToSync.User;

            // Process Shared Inbox
            var followersWtSharedInbox = userWithTweetsToSync.Followers
                .Where(x => !string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithSharedInboxAsync(userWithTweetsToSync.Tweets, followersWtSharedInbox, user);

            // Process Inbox
            var followerWtInbox = userWithTweetsToSync.Followers
                .Where(x => string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithInboxAsync(userWithTweetsToSync.Tweets, followerWtInbox, user);

            return userWithTweetsToSync;
        }

        private async Task ProcessFollowersWithSharedInboxAsync(ExtractedTweet[] tweets, List<Follower> followers, SyncTwitterUser user)
        {
            var followersPerInstances = followers.GroupBy(x => x.Host);

            foreach (var followersPerInstance in followersPerInstances)
            {
                try
                {
                    await _sendTweetsToSharedInbox.ExecuteAsync(tweets, user, followersPerInstance.Key, followersPerInstance.ToArray());

                    foreach (var f in followersPerInstance)
                        await ProcessWorkingUserAsync(f);
                }
                catch (Exception e)
                {
                    var follower = followersPerInstance.First();
                    _logger.LogError(e, "Posting to {Host}{Route} failed", follower.Host, follower.SharedInboxRoute);

                    foreach (var f in followersPerInstance)
                        await ProcessFailingUserAsync(f);
                }
            }
        }
        
        private async Task ProcessFollowersWithInboxAsync(ExtractedTweet[] tweets, List<Follower> followerWtInbox, SyncTwitterUser user)
        {
            foreach (var follower in followerWtInbox)
            {
                try
                {
                    await _sendTweetsToInboxTask.ExecuteAsync(tweets, follower, user);
                    await ProcessWorkingUserAsync(follower);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Posting to {Host}{Route} failed", follower.Host, follower.InboxRoute);
                    await ProcessFailingUserAsync(follower);
                }
            }
        }

        private async Task ProcessWorkingUserAsync(Follower follower)
        {
            if (follower.PostingErrorCount > 0)
            {
                follower.PostingErrorCount = 0;
                await _followersDal.UpdateFollowerAsync(follower);
            }
        }

        private async Task ProcessFailingUserAsync(Follower follower)
        {
            follower.PostingErrorCount++;
            await _followersDal.UpdateFollowerAsync(follower);
        }
    }
}