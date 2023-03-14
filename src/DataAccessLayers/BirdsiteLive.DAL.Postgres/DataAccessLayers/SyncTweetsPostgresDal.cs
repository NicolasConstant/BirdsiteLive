using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using Dapper;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class SyncTweetsPostgresDal : PostgresBase, ISyncTweetsPostgresDal
    {
        #region Ctor
        public SyncTweetsPostgresDal(PostgresSettings settings) : base(settings)
        {
        }
        #endregion

        public async Task<long> SaveTweetAsync(SyncTweet tweet)
        {
            if (tweet.PublishedAt == default) throw new ArgumentException("publishedAt");
            if (tweet.TweetId == default) throw new ArgumentException("tweetId");
            if (string.IsNullOrWhiteSpace(tweet.Acct)) throw new ArgumentException("acct");
            if (string.IsNullOrWhiteSpace(tweet.Inbox)) throw new ArgumentException("inbox");
            if (string.IsNullOrWhiteSpace(tweet.Host)) throw new ArgumentException("host");

            var acct = tweet.Acct.ToLowerInvariant().Trim();
            var inbox = tweet.Inbox.ToLowerInvariant().Trim();
            var host = tweet.Host.ToLowerInvariant().Trim();

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                return (await dbConnection.QueryAsync<long>(
                    $"INSERT INTO {_settings.SynchronizedTweetsTableName} (acct,tweetId,inbox,host,publishedAt) VALUES(@acct,@tweetId,@inbox,@host,@publishedAt) RETURNING id",
                    new
                    {
                        acct,
                        tweetId = tweet.TweetId,
                        inbox,
                        host,
                        publishedAt = tweet.PublishedAt.ToUniversalTime()
                    })).First();
            }
        }

        public async Task<SyncTweet> GetTweetAsync(long id)
        {
            if (id == default) throw new ArgumentException("id");

            var query = $"SELECT * FROM {_settings.SynchronizedTweetsTableName} WHERE id = @id";
            
            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = (await dbConnection.QueryAsync<SyncTweet>(query, new { id })).FirstOrDefault();
                return result;
            }
        }

        public async Task DeleteTweetAsync(long id)
        {
            if (id == default) throw new ArgumentException("id");

            var query = $"DELETE FROM {_settings.SynchronizedTweetsTableName} WHERE id = @id";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                await dbConnection.QueryAsync(query, new { id });
            }
        }

        public async Task<List<SyncTweet>> GetTweetsOlderThanAsync(DateTime date, long from = -1, int size = 100)
        {
            if (date == default) throw new ArgumentException("date");

            var query = $"SELECT * FROM {_settings.SynchronizedTweetsTableName} WHERE id > @from AND publishedAt < @date ORDER BY id ASC LIMIT @size";

            using (var dbConnection = Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<SyncTweet>(query, new
                {
                    from,
                    date,
                    size
                });
                return result.ToList();
            }
        }
    }
}