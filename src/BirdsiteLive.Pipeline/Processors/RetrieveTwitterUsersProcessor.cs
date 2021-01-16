using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveTwitterUsersProcessor : IRetrieveTwitterUsersProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly ILogger<RetrieveTwitterUsersProcessor> _logger;
        private const int SyncPeriod = 15; //in minutes

        #region Ctor
        public RetrieveTwitterUsersProcessor(ITwitterUserDal twitterUserDal, ILogger<RetrieveTwitterUsersProcessor> logger)
        {
            _twitterUserDal = twitterUserDal;
            _logger = logger;
        }
        #endregion

        public async Task GetTwitterUsersAsync(BufferBlock<SyncTwitterUser[]> twitterUsersBufferBlock, CancellationToken ct)
        {
            for (;;)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var users = await _twitterUserDal.GetAllTwitterUsersAsync();

                    if(users.Length > 0)
                        await twitterUsersBufferBlock.SendAsync(users, ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failing retrieving Twitter Users.");
                }

                await Task.Delay(SyncPeriod * 1000 * 60, ct);
            }
        }
    }
}