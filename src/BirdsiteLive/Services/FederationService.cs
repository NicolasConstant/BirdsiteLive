using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class FederationService : BackgroundService
    {
        private readonly IDbInitializerDal _dbInitializerDal;
        private readonly IUserService _userService;

        #region Ctor
        public FederationService(IDbInitializerDal dbInitializerDal, IUserService userService)
        {
            _dbInitializerDal = dbInitializerDal;
            _userService = userService;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DbInitAsync();

            for (;;)
            {
                Console.WriteLine("RUNNING SERVICE");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
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