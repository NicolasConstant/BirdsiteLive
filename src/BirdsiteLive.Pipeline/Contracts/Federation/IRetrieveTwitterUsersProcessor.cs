using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.Pipeline.Contracts.Federation
{
    public interface IRetrieveTwitterUsersProcessor
    {
        Task GetTwitterUsersAsync(BufferBlock<SyncTwitterUser[]> twitterUsersBufferBlock, CancellationToken ct);
    }
}