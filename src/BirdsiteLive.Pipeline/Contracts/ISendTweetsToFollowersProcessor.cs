using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts
{
    public interface ISendTweetsToFollowersProcessor
    {
        Task<UserWithTweetsToSync> ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct);
    }
}