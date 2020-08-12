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
using Tweetinvi.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SendTweetsToFollowersProcessor : ISendTweetsToFollowersProcessor
    {
        private readonly ISendTweetsToInboxTask _sendTweetsToInboxTask;
        private readonly ISendTweetsToSharedInboxTask _sendTweetsToSharedInbox;

        #region Ctor
        public SendTweetsToFollowersProcessor(ISendTweetsToInboxTask sendTweetsToInboxTask, ISendTweetsToSharedInboxTask sendTweetsToSharedInbox)
        {
            _sendTweetsToInboxTask = sendTweetsToInboxTask;
            _sendTweetsToSharedInbox = sendTweetsToSharedInbox;
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
                    await _sendTweetsToSharedInbox.ExecuteAsync(tweets, user, followersPerInstance);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    //TODO handle error
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
                    Console.WriteLine(e);
                    //TODO handle error
                }
            }
        }
    }
}