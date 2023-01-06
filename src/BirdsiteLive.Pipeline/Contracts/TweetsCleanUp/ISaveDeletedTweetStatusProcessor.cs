using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts.TweetsCleanUp
{
    public interface ISaveDeletedTweetStatusProcessor
    {
        Task ProcessAsync(TweetToDelete tweetToDelete, CancellationToken ct);
    }
}