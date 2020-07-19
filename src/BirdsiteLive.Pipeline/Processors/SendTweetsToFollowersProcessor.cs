using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SendTweetsToFollowersProcessor : ISendTweetsToFollowersProcessor
    {
        public Task ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
    }
}