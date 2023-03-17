using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.DAL.Contracts
{
    public interface ISyncTweetsPostgresDal
    {
        Task<long> SaveTweetAsync(SyncTweet tweet);
        Task<SyncTweet> GetTweetAsync(long id);
        Task DeleteTweetAsync(long id);
        Task<List<SyncTweet>> GetTweetsOlderThanAsync(DateTime date, long from = -1, int size = 100);
    }
}