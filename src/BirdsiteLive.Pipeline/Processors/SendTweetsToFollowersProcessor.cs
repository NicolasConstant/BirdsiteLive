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
        private readonly ILogger<SendTweetsToFollowersProcessor> _logger;

        #region Ctor
        public SendTweetsToFollowersProcessor(ISendTweetsToInboxTask sendTweetsToInboxTask, ISendTweetsToSharedInboxTask sendTweetsToSharedInbox, ILogger<SendTweetsToFollowersProcessor> logger)
        {
            _sendTweetsToInboxTask = sendTweetsToInboxTask;
            _sendTweetsToSharedInbox = sendTweetsToSharedInbox;
            _logger = logger;
        }
        #endregion

        public async Task<UserWithTweetsToSync> ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct)
        {
            var user = userWithTweetsToSync.User;

            // Process Shared Inbox
            var followersWtSharedInbox = userWithTweetsToSync.Followers
                .Where(x => !string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithSharedInbox(userWithTweetsToSync.Tweets, followersWtSharedInbox, user);

            // Process Inbox
            var followerWtInbox = userWithTweetsToSync.Followers
                .Where(x => string.IsNullOrWhiteSpace(x.SharedInboxRoute))
                .ToList();
            await ProcessFollowersWithInbox(userWithTweetsToSync.Tweets, followerWtInbox, user);

            return userWithTweetsToSync;
        }

        private async Task ProcessFollowersWithSharedInbox(ExtractedTweet[] tweets, List<Follower> followers, SyncTwitterUser user)
        {
            var followersPerInstances = followers.GroupBy(x => x.Host);

            foreach (var followersPerInstance in followersPerInstances)
            {
                try
                {
                    await _sendTweetsToSharedInbox.ExecuteAsync(tweets, user, followersPerInstance.Key, followersPerInstance.ToArray());
                }
                catch (Exception e)
                {
                    var follower = followersPerInstance.First();
                    _logger.LogError(e, "Posting to {Host}{Route} failed", follower.Host, follower.SharedInboxRoute);
                }
            }
        }
        
        private async Task ProcessFollowersWithInbox(ExtractedTweet[] tweets, List<Follower> followerWtInbox, SyncTwitterUser user)
        {
            foreach (var follower in followerWtInbox)
            {
                try
                {
                    await _sendTweetsToInboxTask.ExecuteAsync(tweets, follower, user);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Posting to {Host}{Route} failed", follower.Host, follower.InboxRoute);
                }
            }
        }
    }
}