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

        #region Ctor
        public FederationService(IDatabaseInitializer databaseInitializer, IStatusPublicationPipeline statusPublicationPipeline)
        {
            _databaseInitializer = databaseInitializer;
            _statusPublicationPipeline = statusPublicationPipeline;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _databaseInitializer.InitAndMigrateDbAsync();
            await _statusPublicationPipeline.ExecuteAsync(stoppingToken);
        }
    }
}