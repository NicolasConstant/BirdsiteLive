using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Settings;
using BirdsiteLive.DAL.Postgres.Tools;
using BirdsiteLive.DAL.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers
{
    [TestClass]
    public class DbInitializerPostgresDalTests
    {
        private readonly PostgresSettings _settings;
        private readonly PostgresTools _tools;

        #region Ctor
        public DbInitializerPostgresDalTests()
        {
            _settings = new PostgresSettings
            {
                ConnString = "Host=127.0.0.1;Username=postgres;Password=mysecretpassword;Database=mytestdb",
                DbVersionTableName = "DbVersionTableName" + RandomGenerator.GetString(4),
                CachedTweetsTableName = "CachedTweetsTableName" + RandomGenerator.GetString(4),
                FollowersTableName = "FollowersTableName" + RandomGenerator.GetString(4),
                TwitterUserTableName = "TwitterUserTableName" + RandomGenerator.GetString(4),
            };
            _tools = new PostgresTools(_settings);
        }
        #endregion

        [TestCleanup]
        public async Task CleanUp()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            try
            {
                await dal.DeleteAllAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [TestMethod]
        public async Task GetCurrentDbVersionAsync_UninitializedDb()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);
            
            var current = await dal.GetCurrentDbVersionAsync();
            Assert.IsNull(current);
        }

        [TestMethod]
        public async Task InitDbAsync()
        {
            var dal = new DbInitializerPostgresDal(_settings, _tools);

            await dal.InitDbAsync();
            var current = await dal.GetCurrentDbVersionAsync();
            var mandatory = dal.GetMandatoryDbVersion();
            Assert.IsNotNull(current);
            Assert.AreEqual(mandatory.Minor, current.Minor);
            Assert.AreEqual(mandatory.Major, current.Major);
        }
    }
}