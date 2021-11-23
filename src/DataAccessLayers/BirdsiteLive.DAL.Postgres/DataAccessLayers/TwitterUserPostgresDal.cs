using System;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using Dapper;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class TwitterUserPostgresDal : PostgresBase, ITwitterUserDal
    {
        #region Ctor
        public TwitterUserPostgresDal(PostgresSettings settings) : base(settings)
        {
            
        }
        #endregion

        public async Task CreateTwitterUserAsync(string acct, long lastTweetPostedId)
        {
            acct = acct.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.TwitterUserTableName} (acct,lastTweetPostedId,lastTweetSynchronizedForAllFollowersId) VALUES(@acct,@lastTweetPostedId,@lastTweetSynchronizedForAllFollowersId)",
                    new { acct, lastTweetPostedId, lastTweetSynchronizedForAllFollowersId = lastTweetPostedId });
            }
        }

        public async Task<SyncTwitterUser> GetTwitterUserAsync(string acct)
        {
            var query = $"SELECT * FROM {_settings.TwitterUserTableName} WHERE acct = @acct";

            acct = acct.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SyncTwitterUser>(query, new { acct })).FirstOrDefault();
                return result;
            }
        }

        public async Task<SyncTwitterUser> GetTwitterUserAsync(int id)
        {
            var query = $"SELECT * FROM {_settings.TwitterUserTableName} WHERE id = @id";
            
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SyncTwitterUser>(query, new { id })).FirstOrDefault();
                return result;
            }
        }

        public async Task<int> GetTwitterUsersCountAsync()
        {
            var query = $"SELECT COUNT(*) FROM {_settings.TwitterUserTableName}";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<int>(query)).FirstOrDefault();
                return result;
            }
        }

        public async Task<int> GetFailingTwitterUsersCountAsync()
        {
            var query = $"SELECT COUNT(*) FROM {_settings.TwitterUserTableName} WHERE fetchingErrorCount > 0";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<int>(query)).FirstOrDefault();
                return result;
            }
        }

        public async Task<SyncTwitterUser[]> GetAllTwitterUsersAsync(int maxNumber)
        {
            var query = $"SELECT * FROM {_settings.TwitterUserTableName} ORDER BY lastSync ASC NULLS FIRST LIMIT @maxNumber";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SyncTwitterUser>(query, new { maxNumber });
                return result.ToArray();
            }
        }

        public async Task<SyncTwitterUser[]> GetAllTwitterUsersAsync()
        {
            var query = $"SELECT * FROM {_settings.TwitterUserTableName}";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SyncTwitterUser>(query);
                return result.ToArray();
            }
        }

        public async Task UpdateTwitterUserAsync(int id, long lastTweetPostedId, long lastTweetSynchronizedForAllFollowersId, int fetchingErrorCount, DateTime lastSync)
        {
            if(id == default) throw new ArgumentException("id");
            if(lastTweetPostedId == default) throw new ArgumentException("lastTweetPostedId");
            if(lastTweetSynchronizedForAllFollowersId == default) throw new ArgumentException("lastTweetSynchronizedForAllFollowersId");
            if(lastSync == default) throw new ArgumentException("lastSync");

            var query = $"UPDATE {_settings.TwitterUserTableName} SET lastTweetPostedId = @lastTweetPostedId, lastTweetSynchronizedForAllFollowersId = @lastTweetSynchronizedForAllFollowersId, fetchingErrorCount = @fetchingErrorCount, lastSync = @lastSync WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id, lastTweetPostedId,  lastTweetSynchronizedForAllFollowersId, fetchingErrorCount, lastSync = lastSync.ToUniversalTime() });
            }
        }

        public async Task UpdateTwitterUserAsync(SyncTwitterUser user)
        {
            await UpdateTwitterUserAsync(user.Id, user.LastTweetPostedId, user.LastTweetSynchronizedForAllFollowersId, user.FetchingErrorCount, user.LastSync);
        }

        public async Task DeleteTwitterUserAsync(string acct)
        {
            if (string.IsNullOrWhiteSpace(acct)) throw new ArgumentException("acct");

            acct = acct.ToLowerInvariant();

            var query = $"DELETE FROM {_settings.TwitterUserTableName} WHERE acct = @acct";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { acct });
            }
        }

        public async Task DeleteTwitterUserAsync(int id)
        {
            if (id == default) throw new ArgumentException("id");
            
            var query = $"DELETE FROM {_settings.TwitterUserTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id });
            }
        }
    }
}