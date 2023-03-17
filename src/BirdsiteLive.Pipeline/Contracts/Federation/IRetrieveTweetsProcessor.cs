using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Contracts.Federation
{
    public interface IRetrieveTweetsProcessor
    {
        Task<UserWithDataToSync[]> ProcessAsync(UserWithDataToSync[] syncTwitterUsers, CancellationToken ct);
    }
}