using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Moderation;
using BirdsiteLive.Pipeline;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class FederationService : BackgroundService
    {
        private readonly IDatabaseInitializer _databaseInitializer;
        private readonly IModerationPipeline _moderationPipeline;
        private readonly IStatusPublicationPipeline _statusPublicationPipeline;

        #region Ctor
        public FederationService(IDatabaseInitializer databaseInitializer, IModerationPipeline moderationPipeline, IStatusPublicationPipeline statusPublicationPipeline)
        {
            _databaseInitializer = databaseInitializer;
            _moderationPipeline = moderationPipeline;
            _statusPublicationPipeline = statusPublicationPipeline;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _databaseInitializer.InitAndMigrateDbAsync();
            await _moderationPipeline.ApplyModerationSettingsAsync();
            await _statusPublicationPipeline.ExecuteAsync(stoppingToken);
        }
    }
}