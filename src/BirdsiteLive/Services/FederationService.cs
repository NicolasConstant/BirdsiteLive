using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class FederationService : BackgroundService
    {
        private readonly IDbInitializerDal _dbInitializerDal;
        private readonly IStatusPublicationPipeline _statusPublicationPipeline;

        #region Ctor
        public FederationService(IDbInitializerDal dbInitializerDal, IStatusPublicationPipeline statusPublicationPipeline)
        {
            _dbInitializerDal = dbInitializerDal;
            _statusPublicationPipeline = statusPublicationPipeline;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DbInitAsync();
            await _statusPublicationPipeline.ExecuteAsync(stoppingToken);
        }

        private async Task DbInitAsync()
        {
            var currentVersion = await _dbInitializerDal.GetCurrentDbVersionAsync();
            var mandatoryVersion = _dbInitializerDal.GetMandatoryDbVersion();

            if (currentVersion == null)
            {
                await _dbInitializerDal.InitDbAsync();
            }
            else if (currentVersion != mandatoryVersion)
            {
                throw new NotImplementedException();
            }
        }
    }
}