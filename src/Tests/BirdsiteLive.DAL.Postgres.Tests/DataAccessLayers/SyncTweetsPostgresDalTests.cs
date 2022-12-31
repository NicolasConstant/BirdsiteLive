using System;
using System.Linq;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class SyncTweetsPostgresDalTests : PostgresTestingBase
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
        public async Task CreateAndGetTweets()
        {
            var tweet = new SyncTweet
            {
                Acct = "test",
                PublishedAt = DateTime.UtcNow,
                Inbox = "https://instance.ext/inbox",
                TweetId = 4567889
            };

            var dal = new SyncTweetsPostgresDal(_settings);

            var id = await dal.SaveTweetAsync(tweet);
            var result = await dal.GetTweetAsync(id);
            
            Assert.IsNotNull(result);

            Assert.IsTrue(result.Id > 0);
            Assert.AreEqual(tweet.Acct, result.Acct);
            Assert.AreEqual(tweet.Inbox, result.Inbox);
            Assert.AreEqual(tweet.TweetId, result.TweetId);
            Assert.IsTrue(Math.Abs((tweet.PublishedAt - result.PublishedAt).Seconds) < 5);
        }

        [TestMethod]
        public async Task CreateDeleteAndGetTweets()
        {
            var tweet = new SyncTweet
            {
                Acct = "test",
                PublishedAt = DateTime.UtcNow,
                Inbox = "https://instance.ext/inbox",
                TweetId = 4567889
            };

            var dal = new SyncTweetsPostgresDal(_settings);

            var id = await dal.SaveTweetAsync(tweet);
            await dal.DeleteTweetAsync(id);
            var result = await dal.GetTweetAsync(id);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task CreateAndGetTweetsByDate()
        {
            var now = DateTime.UtcNow;
            var dal = new SyncTweetsPostgresDal(_settings);

            for (var i = 0; i < 100; i++)
            {
                var tweet = new SyncTweet
                {
                    Acct = "test",
                    PublishedAt = now.AddDays(-10 - i),
                    Inbox = "https://instance.ext/inbox",
                    TweetId = 4567889 + i
                };
                await dal.SaveTweetAsync(tweet);
            }

            var date = now.AddDays(-20);
            var result = await dal.GetTweetsOlderThanAsync(date, -1, 10);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 10);

            foreach (var res in result)
            {
                Assert.IsTrue(res.PublishedAt < date);
                Assert.IsTrue(res.Id > 10);
                Assert.IsTrue(res.Id < 25);
            }
        }

        [TestMethod]
        public async Task CreateAndGetTweetsByDate_Offset()
        {
            var now = DateTime.UtcNow;
            var dal = new SyncTweetsPostgresDal(_settings);

            for (var i = 0; i < 100; i++)
            {
                var tweet = new SyncTweet
                {
                    Acct = "test",
                    PublishedAt = now.AddDays(-10 - i),
                    Inbox = "https://instance.ext/inbox",
                    TweetId = 4567889 + i
                };
                await dal.SaveTweetAsync(tweet);
            }

            var date = now.AddDays(-20);
            var result = await dal.GetTweetsOlderThanAsync(date, 1000, 10);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public async Task CreateAndGetTweetsByDate_Iteration()
        {
            var now = DateTime.UtcNow;
            var dal = new SyncTweetsPostgresDal(_settings);

            for (var i = 0; i < 100; i++)
            {
                var tweet = new SyncTweet
                {
                    Acct = "test",
                    PublishedAt = now.AddDays(-10 - i),
                    Inbox = "https://instance.ext/inbox",
                    TweetId = 4567889 + i
                };
                await dal.SaveTweetAsync(tweet);
            }

            var date = now.AddDays(-20);
            var result = await dal.GetTweetsOlderThanAsync(date, -1, 10);
            var result2 = await dal.GetTweetsOlderThanAsync(date, result.Last().Id, 10);

            var global = result.ToList();
            global.AddRange(result2);
            var d = global.GroupBy(x => x.Id).Count();
            Assert.AreEqual(20, d);

            foreach (var res in global)
            {
                Assert.IsTrue(res.PublishedAt < date);
                Assert.IsTrue(res.Id > 10);
                Assert.IsTrue(res.Id < 35);
            }
        }
    }
}