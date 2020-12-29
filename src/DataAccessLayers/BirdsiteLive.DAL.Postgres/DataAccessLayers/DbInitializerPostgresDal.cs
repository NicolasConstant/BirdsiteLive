using System;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using BirdsiteLive.DAL.Postgres.Tools;
using Dapper;
using Npgsql;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    internal class DbVersion
    {
        public string Type { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
    }

    public class DbInitializerPostgresDal : PostgresBase, IDbInitializerDal
    {
        private readonly PostgresTools _tools;
        private readonly Version _currentVersion = new Version(1,0);
        private const string DbVersionType = "db-version";

        #region Ctor
        public DbInitializerPostgresDal(PostgresSettings settings, PostgresTools tools) : base(settings)
        {
            _tools = tools;
        }
        #endregion
        
        public async Task<Version> GetCurrentDbVersionAsync()
        {
            var query = $"SELECT * FROM {_settings.DbVersionTableName} WHERE type = @type";

            try
            {
                using (var dbConnection = Connection)
                {
                    dbConnection.Open();

                    var result = (await dbConnection.QueryAsync<DbVersion>(query, new { type = DbVersionType })).FirstOrDefault();

                    if (result == default)
                        return null;

                    return new Version(result.Major, result.Minor);
                }
            }
            catch (PostgresException e)
            {
                if (e.Message.Contains("42P01"))
                    return null;

                throw;
            }
        }

        public Version GetMandatoryDbVersion()
        {
            return _currentVersion;
        }

        public Tuple<Version, Version>[] GetMigrationPatterns()
        {
            return new Tuple<Version, Version>[0];
        }

        public Task MigrateDbAsync(Version from, Version to)
        {
            throw new NotImplementedException();
        }

        public async Task InitDbAsync()
        {
            // Create version table 
            var createVersion = $@"CREATE TABLE {_settings.DbVersionTableName}
		    (
                type VARCHAR(128) PRIMARY KEY,

                major SMALLINT NOT NULL,
                minor SMALLINT NOT NULL
		    );";
            await _tools.ExecuteRequestAsync(createVersion);

            // Create Twitter User table
            var createTwitter = $@"CREATE TABLE {_settings.TwitterUserTableName}
		    (
                id SERIAL PRIMARY KEY,
                acct VARCHAR(20) UNIQUE, 

                lastTweetPostedId BIGINT,
                lastTweetSynchronizedForAllFollowersId BIGINT
		    );";
            await _tools.ExecuteRequestAsync(createTwitter);

            // Create Followers table
            var createFollowers = $@"CREATE TABLE {_settings.FollowersTableName}
		    (
                id SERIAL PRIMARY KEY,
                
                followings INTEGER[],
                followingsSyncStatus JSONB,

                acct VARCHAR(50) NOT NULL, 
                host VARCHAR(253) NOT NULL,
                inboxRoute VARCHAR(2048) NOT NULL,
                sharedInboxRoute VARCHAR(2048),
                UNIQUE (acct, host)
		    );";
            await _tools.ExecuteRequestAsync(createFollowers);

            // Create Cached Tweet table 
            var createCachedTweets = $@"CREATE TABLE {_settings.CachedTweetsTableName}
		    (
                id BIGINT PRIMARY KEY,
                twitterUserId INTEGER,
                data JSONB
		    );";
            await _tools.ExecuteRequestAsync(createCachedTweets);

            // Insert version to db
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.DbVersionTableName} (type,major,minor) VALUES(@type,@major,@minor)",
                    new { type = DbVersionType, major = _currentVersion.Major, minor = _currentVersion.Minor });
            }
        }

        public async Task DeleteAllAsync()
        {
            var dropsRequests = new[]
            {
                $@"DROP TABLE {_settings.DbVersionTableName};",
                $@"DROP TABLE {_settings.TwitterUserTableName};",
                $@"DROP TABLE {_settings.FollowersTableName};",
                $@"DROP TABLE {_settings.CachedTweetsTableName};"
            };

            foreach (var r in dropsRequests)
            {
                await _tools.ExecuteRequestAsync(r);
            }
        }
    }
}