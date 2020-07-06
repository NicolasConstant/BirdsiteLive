using BirdsiteLive.DAL.Postgres.Settings;
using BirdsiteLive.DAL.Postgres.Tools;
using BirdsiteLive.DAL.Tools;

namespace BirdsiteLive.DAL.Postgres.Tests.DataAccessLayers.Base
{
    public class PostgresTestingBase
    {
        protected readonly PostgresSettings _settings;
        protected readonly PostgresTools _tools;
        
        #region Ctor
        public PostgresTestingBase()
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
    }
}