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
        private readonly Version _currentVersion = new Version(2, 5);
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

        public async Task<Version> InitDbAsync()
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
            var firstVersion = new Version(1, 0);
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.DbVersionTableName} (type,major,minor) VALUES(@type,@major,@minor)",
                    new { type = DbVersionType, major = firstVersion.Major, minor = firstVersion.Minor });
            }

            return firstVersion;
        }

        public Tuple<Version, Version>[] GetMigrationPatterns()
        {
            return new[]
            {
                new Tuple<Version, Version>(new Version(1,0), new Version(2,0)),
                new Tuple<Version, Version>(new Version(2,0), new Version(2,1)),
                new Tuple<Version, Version>(new Version(2,1), new Version(2,2)),
                new Tuple<Version, Version>(new Version(2,2), new Version(2,3)),
                new Tuple<Version, Version>(new Version(2,3), new Version(2,4)),
                new Tuple<Version, Version>(new Version(2,4), new Version(2,5))
            };
        }

        public async Task<Version> MigrateDbAsync(Version from, Version to)
        {
            if (from == new Version(1, 0) && to == new Version(2, 0))
            {
                var addLastSync = $@"ALTER TABLE {_settings.TwitterUserTableName} ADD lastSync TIMESTAMP (2) WITHOUT TIME ZONE";
                await _tools.ExecuteRequestAsync(addLastSync);

                var addIndex = $@"CREATE INDEX IF NOT EXISTS lastsync_twitteruser ON {_settings.TwitterUserTableName}(lastSync)";
                await _tools.ExecuteRequestAsync(addIndex);
            }
            else if (from == new Version(2, 0) && to == new Version(2, 1))
            {
                var addActorId = $@"ALTER TABLE {_settings.FollowersTableName} ADD actorId VARCHAR(2048)";
                await _tools.ExecuteRequestAsync(addActorId);
            }
            else if (from == new Version(2, 1) && to == new Version(2, 2))
            {
                var addLastSync = $@"ALTER TABLE {_settings.TwitterUserTableName} ADD fetchingErrorCount SMALLINT";
                await _tools.ExecuteRequestAsync(addLastSync);
            }
            else if (from == new Version(2, 2) && to == new Version(2, 3))
            {
                var addPostingError = $@"ALTER TABLE {_settings.FollowersTableName} ADD postingErrorCount SMALLINT";
                await _tools.ExecuteRequestAsync(addPostingError);
            }
            else if (from == new Version(2, 3) && to == new Version(2, 4))
            {
                var alterLastSync = $@"ALTER TABLE {_settings.TwitterUserTableName} ALTER COLUMN fetchingErrorCount TYPE INTEGER";
                await _tools.ExecuteRequestAsync(alterLastSync);

                var alterPostingError = $@"ALTER TABLE {_settings.FollowersTableName} ALTER COLUMN postingErrorCount TYPE INTEGER";
                await _tools.ExecuteRequestAsync(alterPostingError);
            }
            else if (from == new Version(2, 4) && to == new Version(2, 5))
            {
                var addMovedTo = $@"ALTER TABLE {_settings.TwitterUserTableName} ADD movedTo VARCHAR(2048)";
                await _tools.ExecuteRequestAsync(addMovedTo);

                var addMovedToAcct = $@"ALTER TABLE {_settings.TwitterUserTableName} ADD movedToAcct VARCHAR(305)";
                await _tools.ExecuteRequestAsync(addMovedToAcct);

                var addDeletedToAcct = $@"ALTER TABLE {_settings.TwitterUserTableName} ADD deleted BOOLEAN";
                await _tools.ExecuteRequestAsync(addDeletedToAcct);
            }
            else
            {
                throw new NotImplementedException();
            }

            await UpdateDbVersionAsync(to);
            return to;
        }

        private async Task UpdateDbVersionAsync(Version newVersion)
        {
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"UPDATE {_settings.DbVersionTableName} SET major = @major, minor = @minor WHERE type = @type",
                    new { type = DbVersionType, major = newVersion.Major, minor = newVersion.Minor });
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