using System;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.Domain;
using Microsoft.Extensions.Hosting;

namespace BirdsiteLive.Services
{
    public class FederationService : BackgroundService
    {
        private readonly IUserService _userService;

        #region Ctor
        public FederationService(IUserService userService)
        {
            _userService = userService;
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            for (;;)
            {
                Console.WriteLine("RUNNING SERVICE");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}