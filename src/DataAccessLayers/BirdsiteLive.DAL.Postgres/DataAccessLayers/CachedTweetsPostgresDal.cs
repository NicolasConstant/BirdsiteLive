using System;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using BirdsiteLive.DAL.Postgres.Tools;
using Dapper;
using Newtonsoft.Json;
using Tweetinvi.Models;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class CachedTweetsPostgresDal : PostgresBase, ICachedTweetsDal
    {
        #region Ctor
        public CachedTweetsPostgresDal(PostgresSettings settings) : base(settings)
        {
            
        }
        #endregion

        public async Task CreateTweetAsync(long tweetId, int userId, CachedTweet tweet)
        {
            if(tweetId == default) throw new ArgumentException("tweetId");
            if(userId == default) throw new ArgumentException("userId");

            var serializedData = JsonConvert.SerializeObject(tweet);

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.ExecuteAsync(
                    $"INSERT INTO {_settings.CachedTweetsTableName} (id,twitterUserId,data) VALUES(@id,@twitterUserId,CAST(@data as json))",
                    new { id = tweetId, twitterUserId = userId, data = serializedData });
            }
        }

        public async Task<CachedTweet> GetTweetAsync(long tweetId)
        {
            if (tweetId == default) throw new ArgumentException("tweetId");

            var query = $"SELECT * FROM {_settings.CachedTweetsTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SerializedTweet>(query, new { id = tweetId })).FirstOrDefault();
                return Convert(result);
            }
        }

        public async Task DeleteTweetAsync(long tweetId)
        {
            if (tweetId == default) throw new ArgumentException("tweetId");

            var query = $"DELETE FROM {_settings.CachedTweetsTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id = tweetId });
            }
        }

        private CachedTweet Convert(SerializedTweet result)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.Data)) return null;
            return JsonConvert.DeserializeObject<CachedTweet>(result.Data);
        }
    }

    internal class SerializedTweet
    {
        public long Id { get; set; }
        public int TwitterUserId { get; set; }
        public string Data { get; set; }
    }
}