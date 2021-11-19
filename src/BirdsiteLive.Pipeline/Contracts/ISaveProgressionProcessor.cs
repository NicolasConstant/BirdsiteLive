using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts
{
    public interface ISaveProgressionProcessor
    {
        Task ProcessAsync(UserWithDataToSync userWithTweetsToSync, CancellationToken ct);
    }
}