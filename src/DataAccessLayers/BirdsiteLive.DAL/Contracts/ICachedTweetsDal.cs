using System.Threading.Tasks;
using Tweetinvi.Models;

namespace BirdsiteLive.DAL.Contracts
{
    public interface ICachedTweetsDal
    {
        Task AddTweetAsync(long tweetId, int userId, ITweet tweet);
        Task<ITweet> GetTweetAsync(long tweetId);
        Task DeleteTweetAsync(long tweetId);
    }
}