using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class FollowersPostgresDalTests : PostgresTestingBase
    {
        [TestInitialize]
        public async Task TestInit()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            var init = new DatabaseInitializer(dal);
            await init.InitAndMigrateDbAsync();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            await dal.DeleteAllAsync();
        }

        [TestMethod]
        public async Task CreateAndGetFollower()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            var result = await dal.GetFollowerAsync(acct, host);

            Assert.IsNotNull(result);
            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(host, result.Host);
            Assert.AreEqual(inboxRoute, result.InboxRoute);
            Assert.AreEqual(sharedInboxRoute, result.SharedInboxRoute);
            Assert.AreEqual(actorId, result.ActorId);
            Assert.AreEqual(0, result.PostingErrorCount);
            Assert.AreEqual(following.Length, result.Followings.Count);
            Assert.AreEqual(following[0], result.Followings[0]);
            Assert.AreEqual(followingSync.Count, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(followingSync.First().Key, result.FollowingsSyncStatus.First().Key);
            Assert.AreEqual(followingSync.First().Value, result.FollowingsSyncStatus.First().Value);
        }

        [TestMethod]
        public async Task CreateAndGetFollower_NoFollowings()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, null, null);

            var result = await dal.GetFollowerAsync(acct, host);

            Assert.IsNotNull(result);
            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(host, result.Host);
            Assert.AreEqual(actorId, result.ActorId);
            Assert.AreEqual(inboxRoute, result.InboxRoute);
            Assert.AreEqual(sharedInboxRoute, result.SharedInboxRoute);
            Assert.AreEqual(0, result.Followings.Count);
            Assert.AreEqual(0, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(0, result.PostingErrorCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetFollowers_NoId()
        {
            var dal = new FollowersPostgresDal(_settings);
            await dal.GetFollowersAsync(default);
        }

        [TestMethod]
        public async Task CreateAndGetFollower_NoSharedInbox()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            string sharedInboxRoute = null;
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            var result = await dal.GetFollowerAsync(acct, host);

            Assert.IsNotNull(result);
            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(host, result.Host);
            Assert.AreEqual(inboxRoute, result.InboxRoute);
            Assert.AreEqual(actorId, result.ActorId);
            Assert.AreEqual(sharedInboxRoute, result.SharedInboxRoute);
            Assert.AreEqual(following.Length, result.Followings.Count);
            Assert.AreEqual(following[0], result.Followings[0]);
            Assert.AreEqual(followingSync.Count, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(followingSync.First().Key, result.FollowingsSyncStatus.First().Key);
            Assert.AreEqual(followingSync.First().Value, result.FollowingsSyncStatus.First().Value);
            Assert.AreEqual(0, result.PostingErrorCount);
        }

        [TestMethod]
        public async Task GetFollowersAsync()
        {
            var dal = new FollowersPostgresDal(_settings);

            //User 1 
            var acct = "myhandle1";
            var host = "domain.ext";
            var following = new[] { 1, 2, 3 };
            var followingSync = new Dictionary<int, long>();
            var inboxRoute = "/myhandle1/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle2";
            host = "domain.ext";
            following = new[] { 2, 4, 5 };
            inboxRoute = "/myhandle2/inbox";
            sharedInboxRoute = "/inbox2";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle3";
            host = "domain.ext";
            following = new[] { 1 };
            inboxRoute = "/myhandle3/inbox";
            sharedInboxRoute = "/inbox3";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            var result = await dal.GetFollowersAsync(2);
            Assert.AreEqual(2, result.Length);

            result = await dal.GetFollowersAsync(5);
            Assert.AreEqual(1, result.Length);

            result = await dal.GetFollowersAsync(24);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public async Task GetAllFollowersAsync()
        {
            var dal = new FollowersPostgresDal(_settings);

            //User 1 
            var acct = "myhandle1";
            var host = "domain.ext";
            var following = new[] { 1, 2, 3 };
            var followingSync = new Dictionary<int, long>();
            var inboxRoute = "/myhandle1/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle2";
            host = "domain.ext";
            following = new[] { 2, 4, 5 };
            inboxRoute = "/myhandle2/inbox";
            sharedInboxRoute = "/inbox2";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle3";
            host = "domain.ext";
            following = new[] { 1 };
            inboxRoute = "/myhandle3/inbox";
            sharedInboxRoute = "/inbox3";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            var result = await dal.GetAllFollowersAsync();
            Assert.AreEqual(3, result.Length);
        }

        [TestMethod]
        public async Task CountFollowersAsync()
        {
            var dal = new FollowersPostgresDal(_settings);

            var result = await dal.GetFollowersCountAsync();
            Assert.AreEqual(0, result);

            //User 1 
            var acct = "myhandle1";
            var host = "domain.ext";
            var following = new[] { 1, 2, 3 };
            var followingSync = new Dictionary<int, long>();
            var inboxRoute = "/myhandle1/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle2";
            host = "domain.ext";
            following = new[] { 2, 4, 5 };
            inboxRoute = "/myhandle2/inbox";
            sharedInboxRoute = "/inbox2";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 3
            acct = "myhandle3";
            host = "domain.ext";
            following = new[] { 1 };
            inboxRoute = "/myhandle3/inbox";
            sharedInboxRoute = "/inbox3";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            result = await dal.GetFollowersCountAsync();
            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public async Task CountFailingFollowersAsync()
        {
            var dal = new FollowersPostgresDal(_settings);

            var result = await dal.GetFailingFollowersCountAsync();
            Assert.AreEqual(0, result);

            //User 1 
            var acct = "myhandle1";
            var host = "domain.ext";
            var following = new[] { 1, 2, 3 };
            var followingSync = new Dictionary<int, long>();
            var inboxRoute = "/myhandle1/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            //User 2 
            acct = "myhandle2";
            host = "domain.ext";
            following = new[] { 2, 4, 5 };
            inboxRoute = "/myhandle2/inbox";
            sharedInboxRoute = "/inbox2";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            var follower = await dal.GetFollowerAsync(acct, host);
            follower.PostingErrorCount = 1;
            await dal.UpdateFollowerAsync(follower);

            //User 3
            acct = "myhandle3";
            host = "domain.ext";
            following = new[] { 1 };
            inboxRoute = "/myhandle3/inbox";
            sharedInboxRoute = "/inbox3";
            actorId = $"https://{host}/{acct}";
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);

            follower = await dal.GetFollowerAsync(acct, host);
            follower.PostingErrorCount = 50;
            await dal.UpdateFollowerAsync(follower);

            result = await dal.GetFailingFollowersCountAsync();
            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetFollower_Add()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);

            var updatedFollowing = new List<int> { 12, 19, 23, 24 };
            var updatedFollowingSync = new Dictionary<int, long>(){
                {12, 170L},
                {19, 171L},
                {23, 172L},
                {24, 173L}
            };
            result.Followings = updatedFollowing.ToList();
            result.FollowingsSyncStatus = updatedFollowingSync;
            result.PostingErrorCount = 10;
            
            await dal.UpdateFollowerAsync(result);
            result = await dal.GetFollowerAsync(acct, host);

            Assert.AreEqual(updatedFollowing.Count, result.Followings.Count);
            Assert.AreEqual(updatedFollowing[0], result.Followings[0]);
            Assert.AreEqual(updatedFollowingSync.Count, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(updatedFollowingSync.First().Key, result.FollowingsSyncStatus.First().Key);
            Assert.AreEqual(updatedFollowingSync.First().Value, result.FollowingsSyncStatus.First().Value);
            Assert.AreEqual(10, result.PostingErrorCount);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetFollower_Integer()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);

            var updatedFollowing = new List<int> { 12, 19, 23, 24 };
            var updatedFollowingSync = new Dictionary<int, long>(){
                {12, 170L},
                {19, 171L},
                {23, 172L},
                {24, 173L}
            };
            result.Followings = updatedFollowing.ToList();
            result.FollowingsSyncStatus = updatedFollowingSync;
            result.PostingErrorCount = 32768;

            await dal.UpdateFollowerAsync(result);
            result = await dal.GetFollowerAsync(acct, host);

            Assert.AreEqual(updatedFollowing.Count, result.Followings.Count);
            Assert.AreEqual(updatedFollowing[0], result.Followings[0]);
            Assert.AreEqual(updatedFollowingSync.Count, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(updatedFollowingSync.First().Key, result.FollowingsSyncStatus.First().Key);
            Assert.AreEqual(updatedFollowingSync.First().Value, result.FollowingsSyncStatus.First().Value);
            Assert.AreEqual(32768, result.PostingErrorCount);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetFollower_Remove()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);

            var updatedFollowing = new[] { 12, 19 };
            var updatedFollowingSync = new Dictionary<int, long>()
            {
                {12, 170L},
                {19, 171L}
            };
            result.Followings = updatedFollowing.ToList();
            result.FollowingsSyncStatus = updatedFollowingSync;
            result.PostingErrorCount = 5;

            await dal.UpdateFollowerAsync(result);
            result = await dal.GetFollowerAsync(acct, host);

            Assert.AreEqual(updatedFollowing.Length, result.Followings.Count);
            Assert.AreEqual(updatedFollowing[0], result.Followings[0]);
            Assert.AreEqual(updatedFollowingSync.Count, result.FollowingsSyncStatus.Count);
            Assert.AreEqual(updatedFollowingSync.First().Key, result.FollowingsSyncStatus.First().Key);
            Assert.AreEqual(updatedFollowingSync.First().Value, result.FollowingsSyncStatus.First().Value);
            Assert.AreEqual(5, result.PostingErrorCount);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetFollower_ResetErrorCount()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);
            Assert.AreEqual(0, result.PostingErrorCount);

            result.PostingErrorCount = 5;

            await dal.UpdateFollowerAsync(result);
            result = await dal.GetFollowerAsync(acct, host);
            Assert.AreEqual(5, result.PostingErrorCount);

            result.PostingErrorCount = 0;

            await dal.UpdateFollowerAsync(result);
            result = await dal.GetFollowerAsync(acct, host);
            Assert.AreEqual(0, result.PostingErrorCount);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoFollower()
        {
            var dal = new FollowersPostgresDal(_settings);
            await dal.UpdateFollowerAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoFollowerId()
        {
            var follower = new Follower
            {
                Id = default
            };

            var dal = new FollowersPostgresDal(_settings);
            await dal.UpdateFollowerAsync(follower);
        }

        [TestMethod]
        public async Task CreateAndDeleteFollower_ById()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);
            Assert.IsNotNull(result);

            await dal.DeleteFollowerAsync(result.Id);

            result = await dal.GetFollowerAsync(acct, host);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAndDeleteFollower_ByHandle()
        {
            var acct = "myhandle";
            var host = "domain.ext";
            var following = new[] { 12, 19, 23 };
            var followingSync = new Dictionary<int, long>()
            {
                {12, 165L},
                {19, 166L},
                {23, 167L}
            };
            var inboxRoute = "/myhandle/inbox";
            var sharedInboxRoute = "/inbox";
            var actorId = $"https://{host}/{acct}";

            var dal = new FollowersPostgresDal(_settings);
            await dal.CreateFollowerAsync(acct, host, inboxRoute, sharedInboxRoute, actorId, following, followingSync);
            var result = await dal.GetFollowerAsync(acct, host);
            Assert.IsNotNull(result);

            await dal.DeleteFollowerAsync(acct, host);

            result = await dal.GetFollowerAsync(acct, host);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Delete_NoFollowerId()
        {
            var dal = new FollowersPostgresDal(_settings);
            await dal.DeleteFollowerAsync(default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Delete_NoAcct()
        {
            var dal = new FollowersPostgresDal(_settings);
            await dal.DeleteFollowerAsync(string.Empty, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Delete_NoHost()
        {
            var dal = new FollowersPostgresDal(_settings);
            await dal.DeleteFollowerAsync("acct", string.Empty);
        }
    }
}