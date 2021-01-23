using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class CachedTweetsPostgresDalTests : PostgresTestingBase
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
        public async Task CreateAndGet()
        {
            var id = 152L;
            var userId = 15;

            var tweet = new CachedTweet
            {
                UserId = userId,
                Id = id,
                Text = "text data",
                FullText = "full text data",
                CreatedAt = DateTime.UtcNow
            };

            var dal = new CachedTweetsPostgresDal(_settings);
            await dal.CreateTweetAsync(id, userId, tweet);

            var result = await dal.GetTweetAsync(id);

            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(tweet.Text, result.Text);
            Assert.AreEqual(tweet.FullText, result.FullText);
            Assert.AreEqual(tweet.CreatedAt, result.CreatedAt);
        }

        [TestMethod]
        public async Task CreateAndDelete()
        {
            var id = 152L;
            var userId = 15;

            var tweet = new CachedTweet
            {
                UserId = userId,
                Id = id,
                Text = "text data",
                FullText = "full text data",
                CreatedAt = DateTime.UtcNow
            };

            var dal = new CachedTweetsPostgresDal(_settings);
            await dal.CreateTweetAsync(id, userId, tweet);

            var result = await dal.GetTweetAsync(id);
            Assert.IsNotNull(result);

            await dal.DeleteTweetAsync(id);
            result = await dal.GetTweetAsync(id);
            Assert.IsNull(result);
        }
    }
}