using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using Dapper;
using Newtonsoft.Json;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class FollowersPostgresDal : PostgresBase, IFollowersDal
    {
        #region Ctor
        public FollowersPostgresDal(PostgresSettings settings) : base(settings)
        {
            
        }
        #endregion

        public async Task CreateFollowerAsync(string acct, string host, string inboxRoute, string sharedInboxRoute, string actorId, int[] followings = null, Dictionary<int, long> followingSyncStatus = null)
        {
            if(followings == null) followings = new int[0];
            if(followingSyncStatus == null) followingSyncStatus = new Dictionary<int, long>();

            var serializedDic = JsonConvert.SerializeObject(followingSyncStatus);

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.FollowersTableName} (acct,host,inboxRoute,sharedInboxRoute,followings,followingsSyncStatus,actorId) VALUES(@acct,@host,@inboxRoute,@sharedInboxRoute,@followings,CAST(@followingsSyncStatus as json),@actorId)",
                    new { acct, host, inboxRoute, sharedInboxRoute, followings, followingsSyncStatus = serializedDic, actorId });
            }
        }

        public async Task<int> GetFollowersCountAsync()
        {
            var query = $"SELECT COUNT(*) FROM {_settings.FollowersTableName}";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<int>(query)).FirstOrDefault();
                return result;
            }
        }

        public async Task<int> GetFailingFollowersCountAsync()
        {
            var query = $"SELECT COUNT(*) FROM {_settings.FollowersTableName} WHERE postingErrorCount > 0";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<int>(query)).FirstOrDefault();
                return result;
            }
        }

        public async Task<Follower> GetFollowerAsync(string acct, string host)
        {
            var query = $"SELECT * FROM {_settings.FollowersTableName} WHERE acct = @acct AND host = @host";

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SerializedFollower>(query, new { acct, host })).FirstOrDefault();
                return Convert(result);
            }
        }

        public async Task<Follower[]> GetFollowersAsync(int followedUserId)
        {
            if (followedUserId == default) throw new ArgumentException("followedUserId");

            var query = $"SELECT * FROM {_settings.FollowersTableName} WHERE @id=ANY(followings)";
            
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SerializedFollower>(query, new { id = followedUserId});
                return result.Select(Convert).ToArray();
            }
        }

        public async Task<Follower[]> GetAllFollowersAsync()
        {
            var query = $"SELECT * FROM {_settings.FollowersTableName}";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SerializedFollower>(query);
                return result.Select(Convert).ToArray();
            }
        }

        public async Task UpdateFollowerAsync(Follower follower)
        {
            if (follower == default) throw new ArgumentException("follower");
            if (follower.Id == default) throw new ArgumentException("id");

            var serializedDic = JsonConvert.SerializeObject(follower.FollowingsSyncStatus);
            var query = $"UPDATE {_settings.FollowersTableName} SET followings = @followings, followingsSyncStatus = CAST(@followingsSyncStatus as json), postingErrorCount = @postingErrorCount WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { follower.Id, follower.Followings, followingsSyncStatus = serializedDic, postingErrorCount = follower.PostingErrorCount });
            }
        }

        public async Task DeleteFollowerAsync(int id)
        {
            if (id == default) throw new ArgumentException("id");
            
            var query = $"DELETE FROM {_settings.FollowersTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id });
            }
        }

        public async Task DeleteFollowerAsync(string acct, string host)
        {
            if (string.IsNullOrWhiteSpace(acct)) throw new ArgumentException("acct");
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("host");

            acct = acct.ToLowerInvariant();
            host = host.ToLowerInvariant();

            var query = $"DELETE FROM {_settings.FollowersTableName} WHERE acct = @acct AND host = @host";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { acct, host });
            }
        }

        private Follower Convert(SerializedFollower follower)
        {
            if (follower == null) return null;

            return new Follower()
            {
                Id = follower.Id,
                Acct = follower.Acct,
                Host = follower.Host,
                InboxRoute = follower.InboxRoute,
                ActorId = follower.ActorId,
                SharedInboxRoute = follower.SharedInboxRoute,
                Followings = follower.Followings.ToList(),
                FollowingsSyncStatus = JsonConvert.DeserializeObject<Dictionary<int,long>>(follower.FollowingsSyncStatus),
                PostingErrorCount = follower.PostingErrorCount
            };
        }
    }

    internal class SerializedFollower {
        public int Id { get; set; }

        public int[] Followings { get; set; }
        public string FollowingsSyncStatus { get; set; }

        public string Acct { get; set; }
        public string Host { get; set; }
        public string InboxRoute { get; set; }
        public string SharedInboxRoute { get; set; }
        public string ActorId { get; set; }
        public int PostingErrorCount { get; set; }
    }
}