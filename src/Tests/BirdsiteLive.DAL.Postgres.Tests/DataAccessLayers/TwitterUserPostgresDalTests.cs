using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class TwitterUserPostgresDalTests : PostgresTestingBase
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
        public async Task GetTwitterUserAsync_NoUser()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            var result = await dal.GetTwitterUserAsync("dontexist");
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(lastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(lastTweetId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(0, result.FetchingErrorCount);
            Assert.IsTrue(result.Id > 0);
        }

        [TestMethod]
        public async Task CreateAndGetUser_byId()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);
            var resultById = await dal.GetTwitterUserAsync(result.Id);

            Assert.AreEqual(acct, resultById.Acct);
            Assert.AreEqual(lastTweetId, resultById.LastTweetPostedId);
            Assert.AreEqual(lastTweetId, resultById.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(result.Id, resultById.Id);
        }

        [TestMethod]
        public async Task CreateAndGetMigratedUser_byId()
        {
            var acct = "myid";
            var lastTweetId = 1548L;
            var movedTo = "https://";
            var movedToAcct = "@account@instance";

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId, movedTo, movedToAcct);
            var result = await dal.GetTwitterUserAsync(acct);
            var resultById = await dal.GetTwitterUserAsync(result.Id);

            Assert.AreEqual(acct, resultById.Acct);
            Assert.AreEqual(lastTweetId, resultById.LastTweetPostedId);
            Assert.AreEqual(lastTweetId, resultById.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(result.Id, resultById.Id);
            Assert.AreEqual(movedTo, result.MovedTo);
            Assert.AreEqual(movedToAcct, result.MovedToAcct);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);


            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            var errors = 15;
            await dal.UpdateTwitterUserAsync(result.Id, updatedLastTweetId, updatedLastSyncId, errors, now, null, null, false);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(errors, result.FetchingErrorCount);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
            Assert.AreEqual(null, result.MovedTo);
            Assert.AreEqual(null, result.MovedToAcct);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetMigratedUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);


            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            var errors = 15;
            var movedTo = "https://";
            var movedToAcct = "@account@instance";
            await dal.UpdateTwitterUserAsync(result.Id, updatedLastTweetId, updatedLastSyncId, errors, now, movedTo, movedToAcct, false);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(errors, result.FetchingErrorCount);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
            Assert.AreEqual(movedTo, result.MovedTo);
            Assert.AreEqual(movedToAcct, result.MovedToAcct);
        }

        [TestMethod]
        public async Task CreateUpdateAndGetDeletedUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);

            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            var errors = 15;
            await dal.UpdateTwitterUserAsync(result.Id, updatedLastTweetId, updatedLastSyncId, errors, now, null, null, true);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(errors, result.FetchingErrorCount);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
            Assert.AreEqual(null, result.MovedTo);
            Assert.AreEqual(null, result.MovedToAcct);
            Assert.AreEqual(true, result.Deleted);
        }

        [TestMethod]
        public async Task CreateUpdate2AndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);


            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            var errors = 15;

            result.LastTweetPostedId = updatedLastTweetId;
            result.LastTweetSynchronizedForAllFollowersId = updatedLastSyncId;
            result.FetchingErrorCount = errors;
            result.LastSync = now;
            await dal.UpdateTwitterUserAsync(result);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(errors, result.FetchingErrorCount);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
        }

        [TestMethod]
        public async Task CreateUpdate3AndGetUser()
        {
            var acct = "myid";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);


            var updatedLastTweetId = 1600L;
            var updatedLastSyncId = 1550L;
            var now = DateTime.Now;
            var errors = 32768;

            result.LastTweetPostedId = updatedLastTweetId;
            result.LastTweetSynchronizedForAllFollowersId = updatedLastSyncId;
            result.FetchingErrorCount = errors;
            result.LastSync = now;
            await dal.UpdateTwitterUserAsync(result);

            result = await dal.GetTwitterUserAsync(acct);

            Assert.AreEqual(acct, result.Acct);
            Assert.AreEqual(updatedLastTweetId, result.LastTweetPostedId);
            Assert.AreEqual(updatedLastSyncId, result.LastTweetSynchronizedForAllFollowersId);
            Assert.AreEqual(errors, result.FetchingErrorCount);
            Assert.IsTrue(Math.Abs((now.ToUniversalTime() - result.LastSync).Milliseconds) < 100);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoId()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.UpdateTwitterUserAsync(default, default, default, default, DateTime.UtcNow, null, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoLastTweetPostedId()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.UpdateTwitterUserAsync(12, default, default, default, DateTime.UtcNow, null, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoLastTweetSynchronizedForAllFollowersId()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.UpdateTwitterUserAsync(12, 9556, default, default, DateTime.UtcNow, null, null, false);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Update_NoLastSync()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.UpdateTwitterUserAsync(12, 9556, 65, default, default, null, null, false);
        }

        [TestMethod]
        public async Task CreateAndDeleteUser()
        {
            var acct = "myacct";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNotNull(result);

            await dal.DeleteTwitterUserAsync(acct);
            result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteUser_NotAcct()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.DeleteTwitterUserAsync(string.Empty);
        }

        [TestMethod]
        public async Task CreateAndDeleteUser_byId()
        {
            var acct = "myacct";
            var lastTweetId = 1548L;

            var dal = new TwitterUserPostgresDal(_settings);

            await dal.CreateTwitterUserAsync(acct, lastTweetId);
            var result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNotNull(result);

            await dal.DeleteTwitterUserAsync(result.Id);
            result = await dal.GetTwitterUserAsync(acct);
            Assert.IsNull(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task DeleteUser_NotAcct_byId()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            await dal.DeleteTwitterUserAsync(default(int));
        }

        [TestMethod]
        public async Task GetAllTwitterUsers_Top()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 1000; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            for (int i = 0; i < 10; i++)
            {
                var acct = $"migrated-myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId, "https://url/account", "@user@domain");
            }

            for (int i = 0; i < 10; i++)
            {
                var acct = $"deleted-myid{i}";
                var lastTweetId = 148L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
                var user = await dal.GetTwitterUserAsync(acct);
                user.Deleted = true;
                user.LastSync = DateTime.UtcNow;
                await dal.UpdateTwitterUserAsync(user);
            }

            var result = await dal.GetAllTwitterUsersAsync(1100, false);
            Assert.AreEqual(1000, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);

            foreach (var user in result)
            {
                Assert.IsTrue(string.IsNullOrWhiteSpace(user.MovedTo));
                Assert.IsTrue(string.IsNullOrWhiteSpace(user.MovedToAcct));
                Assert.IsFalse(user.Deleted);
            }
        }

        [TestMethod]
        public async Task GetAllTwitterUsers_Top_RetrieveDeleted()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 1000; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            for (int i = 0; i < 10; i++)
            {
                var acct = $"migrated-myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId, "https://url/account", "@user@domain");
            }

            for (int i = 0; i < 10; i++)
            {
                var acct = $"deleted-myid{i}";
                var lastTweetId = 148L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
                var user = await dal.GetTwitterUserAsync(acct);
                user.Deleted = true;
                user.LastSync = DateTime.UtcNow;
                await dal.UpdateTwitterUserAsync(user);
            }

            var result = await dal.GetAllTwitterUsersAsync(1100, true);
            Assert.AreEqual(1020, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);
        }

        [TestMethod]
        public async Task GetAllTwitterUsers_Top_NotInit()
        {
            // Create accounts
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 1000; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = i+10;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            // Update accounts
            var now = DateTime.UtcNow;
            var allUsers = await dal.GetAllTwitterUsersAsync(false);
            foreach (var acc in allUsers)
            {
                var lastSync = now.AddDays(acc.LastTweetPostedId);
                acc.LastSync = lastSync;
                await dal.UpdateTwitterUserAsync(acc);
            }

            // Create a not init account
            await dal.CreateTwitterUserAsync("not_init", -1);

            var result = await dal.GetAllTwitterUsersAsync(10, false);

            Assert.IsTrue(result.Any(x => x.Acct == "myid0"));
            Assert.IsTrue(result.Any(x => x.Acct == "myid8"));
            Assert.IsTrue(result.Any(x => x.Acct == "not_init"));
        }

        [TestMethod]
        public async Task GetAllTwitterUsers_Limited()
        {
            var now = DateTime.Now;
            var oldest = now.AddDays(-3);
            var newest = now.AddDays(-2);

            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 20; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            var allUsers = await dal.GetAllTwitterUsersAsync(100, false);
            for (var i = 0; i < 20; i++)
            {
                var user = allUsers[i];
                var date = i % 2 == 0 ? oldest : newest;
                await dal.UpdateTwitterUserAsync(user.Id, user.LastTweetPostedId, user.LastTweetSynchronizedForAllFollowersId, 0, date, null, null, false);
            }

            var result = await dal.GetAllTwitterUsersAsync(10, false);
            Assert.AreEqual(10, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);

            foreach (var acc in result)
                Assert.IsTrue(Math.Abs((acc.LastSync - oldest.ToUniversalTime()).TotalMilliseconds) < 1000);
        }

        [TestMethod]
        public async Task GetAllTwitterUsers()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 1000; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            for (int i = 0; i < 10; i++)
            {
                var acct = $"migrated-myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId, "https://url/account", "@user@domain");
            }

            var result = await dal.GetAllTwitterUsersAsync(false);
            Assert.AreEqual(1000, result.Length);
            Assert.IsFalse(result[0].Id == default);
            Assert.IsFalse(result[0].Acct == default);
            Assert.IsFalse(result[0].LastTweetPostedId == default);
            Assert.IsFalse(result[0].LastTweetSynchronizedForAllFollowersId == default);
        }

        [TestMethod]
        public async Task CountTwitterUsers()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 10; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);
            }

            var result = await dal.GetTwitterUsersCountAsync();
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public async Task CountFailingTwitterUsers()
        {
            var dal = new TwitterUserPostgresDal(_settings);
            for (var i = 0; i < 10; i++)
            {
                var acct = $"myid{i}";
                var lastTweetId = 1548L;

                await dal.CreateTwitterUserAsync(acct, lastTweetId);

                if (i == 0 || i == 2 || i == 3)
                {
                    var t = await dal.GetTwitterUserAsync(acct);
                    await dal.UpdateTwitterUserAsync(t.Id ,1L,2L, 50+i*2, DateTime.Now, null, null, false);
                }
            }

            var result = await dal.GetFailingTwitterUsersCountAsync();
            Assert.AreEqual(3, result);
        }
    }
}