using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveTweetsProcessor : IRetrieveTweetsProcessor
    {
        public Task<UserWithTweetsToSync[]> ProcessAsync(SyncTwitterUser[] syncTwitterUsers, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
    }
}