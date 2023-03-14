using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts.TweetsCleanUp
{
    public interface IRetrieveTweetsToDeleteProcessor
    {
        Task GetTweetsAsync(BufferBlock<TweetToDelete> tweetsBufferBlock, CancellationToken ct);
    }
}