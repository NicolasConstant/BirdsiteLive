using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts
{
    public interface IRetrieveFollowersProcessor
    {
        Task<IEnumerable<UserWithDataToSync>> ProcessAsync(UserWithDataToSync[] userWithTweetsToSyncs, CancellationToken ct);
        //IAsyncEnumerable<UserWithTweetsToSync> ProcessAsync(UserWithTweetsToSync[] userWithTweetsToSyncs, CancellationToken ct);
    }
}