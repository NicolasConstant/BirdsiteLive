using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts.Federation
{
    public interface IRefreshTwitterUserStatusProcessor
    {
        Task<UserWithDataToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct);
    }
}