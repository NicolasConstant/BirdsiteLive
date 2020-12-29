using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts
{
    public interface IRetrieveTweetsProcessor
    {
        Task<UserWithTweetsToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct);
    }
}