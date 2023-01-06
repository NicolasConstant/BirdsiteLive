using System.Threading.Tasks;
using System.Threading;

namespace BirdsiteLive.Pipeline
{
    public interface ITweetCleanUpPipeline
    {
        Task ExecuteAsync(CancellationToken ct);
    }

    public class TweetCleanUpPipeline : ITweetCleanUpPipeline
    {
        public async Task ExecuteAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
    }
}