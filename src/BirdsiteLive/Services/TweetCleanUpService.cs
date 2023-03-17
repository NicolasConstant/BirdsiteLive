using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Pipeline;
using BirdsiteLive.Tools;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class TweetCleanUpService : BackgroundService
    {
        private readonly ITweetCleanUpPipeline _cleanUpPipeline;
        private readonly IHostApplicationLifetime _applicationLifetime;

        #region Ctor
        public TweetCleanUpService(IHostApplicationLifetime applicationLifetime, ITweetCleanUpPipeline cleanUpPipeline)
        {
            _applicationLifetime = applicationLifetime;
            _cleanUpPipeline = cleanUpPipeline;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Wait for initialization
                while (!InitStateSynchronization.IsDbInitialized)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    await Task.Delay(250, stoppingToken);
                }

                await _cleanUpPipeline.ExecuteAsync(stoppingToken);
            }
            catch (Exception)
            {
                await Task.Delay(1000 * 30);
                _applicationLifetime.StopApplication();
            }
        }
    }
}