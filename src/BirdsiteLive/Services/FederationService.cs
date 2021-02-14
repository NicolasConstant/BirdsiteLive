using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class FederationService : BackgroundService
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly IStatusPublicationPipeline _statusPublicationPipeline;
        private readonly IHostApplicationLifetime _applicationLifetime;

        #region Ctor
        public FederationService(IDatabaseInitializer databaseInitializer, IStatusPublicationPipeline statusPublicationPipeline, IHostApplicationLifetime applicationLifetime)
        {
            _databaseInitializer = databaseInitializer;
            _statusPublicationPipeline = statusPublicationPipeline;
            _applicationLifetime = applicationLifetime;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _databaseInitializer.InitAndMigrateDbAsync();
                await _statusPublicationPipeline.ExecuteAsync(stoppingToken);
            }
            finally
            {
                _applicationLifetime.StopApplication();
            }
        }
    }
}