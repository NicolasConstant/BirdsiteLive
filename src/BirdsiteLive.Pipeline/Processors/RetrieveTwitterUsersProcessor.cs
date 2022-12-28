using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BirdsiteLive.Common.Extensions;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Tools;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveTwitterUsersProcessor : IRetrieveTwitterUsersProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IMaxUsersNumberProvider _maxUsersNumberProvider;
        private readonly ILogger<RetrieveTwitterUsersProcessor> _logger;
        
        public int WaitFactor = 1000 * 60; //1 min

        #region Ctor
        public RetrieveTwitterUsersProcessor(ITwitterUserDal twitterUserDal, IMaxUsersNumberProvider maxUsersNumberProvider, ILogger<RetrieveTwitterUsersProcessor> logger)
        {
            _twitterUserDal = twitterUserDal;
            _maxUsersNumberProvider = maxUsersNumberProvider;
            _logger = logger;
        }
        #endregion

        public async Task GetTwitterUsersAsync(BufferBlock<SyncTwitterUser[]> twitterUsersBufferBlock, CancellationToken ct)
        {
            for (; ; )
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    var maxUsersNumber = await _maxUsersNumberProvider.GetMaxUsersNumberAsync();
                    var users = await _twitterUserDal.GetAllTwitterUsersAsync(maxUsersNumber, false);

                    var userCount = users.Any() ? users.Length : 1;
                    var splitNumber = (int) Math.Ceiling(userCount / 15d);
                    var splitUsers = users.Split(splitNumber).ToList();

                    foreach (var u in splitUsers)
                    {
                        ct.ThrowIfCancellationRequested();

                        await twitterUsersBufferBlock.SendAsync(u.ToArray(), ct);

                        await Task.Delay(WaitFactor, ct);
                    }

                    var splitCount = splitUsers.Count();
                    if (splitCount < 15) await Task.Delay((15 - splitCount) * WaitFactor, ct); //Always wait 15min

                    //// Extra wait time to fit 100.000/day limit
                    //var extraWaitTime = (int)Math.Ceiling((60 / ((100000d / 24) / userCount)) - 15);
                    //if (extraWaitTime < 0) extraWaitTime = 0;
                    //await Task.Delay(extraWaitTime * 1000, ct);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failing retrieving Twitter Users.");
                }
            }
        }
    }
}