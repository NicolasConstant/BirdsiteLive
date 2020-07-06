using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Postgres.DataAccessLayers.Base;
using BirdsiteLive.DAL.Postgres.Settings;
using BirdsiteLive.DAL.Postgres.Tools;
using Tweetinvi.Models;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers
{
    public class CachedTweetsPostgresDal : PostgresBase, ICachedTweetsDal
    {
        #region Ctor
        public CachedTweetsPostgresDal(PostgresSettings settings, PostgresTools tools) : base(settings)
        {
            
        }
        #endregion

        public Task AddTweetAsync(long tweetId, int userId, ITweet tweet)
        {
            throw new System.NotImplementedException();
        }

        public Task<ITweet> GetTweetAsync(long tweetId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteTweetAsync(long tweetId)
        {
            throw new System.NotImplementedException();
        }
    }
}