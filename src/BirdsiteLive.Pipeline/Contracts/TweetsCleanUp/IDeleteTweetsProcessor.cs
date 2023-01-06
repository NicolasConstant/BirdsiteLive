using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts.TweetsCleanUp
{
    public interface IDeleteTweetsProcessor
    {
        Task<TweetToDelete> ProcessAsync(TweetToDelete tweet, CancellationToken ct);
    }
}