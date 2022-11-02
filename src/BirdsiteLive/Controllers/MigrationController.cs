using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql.TypeHandlers;
using BirdsiteLive.Domain;

namespace BirdsiteLive.Controllers
{
    public class MigrationController : Controller
    {
        private readonly MigrationService _migrationService;

        #region Ctor
        public MigrationController(MigrationService migrationService)
        {
            _migrationService = migrationService;
        }
        #endregion

        [HttpGet]
        [Route("/migration/{id}")]
        public IActionResult Index(string id)
        {
            var migrationCode = _migrationService.GetMigrationCode(id);
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode
            };

            return View(data);
        }

        [HttpPost]
        [Route("/migration/{id}")]
        public async Task<IActionResult> Migrate(string id, string tweetid, string handle)
        {
            var migrationCode = _migrationService.GetMigrationCode(id);
            
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode,

                IsAcctProvided = !string.IsNullOrWhiteSpace(handle),
                IsTweetProvided = !string.IsNullOrWhiteSpace(tweetid),

                TweetId = tweetid,
                FediverseAccount = handle
            };

            try
            {
                var isAcctValid = await _migrationService.ValidateFediverseAcctAsync(handle);
                var isTweetValid = _migrationService.ValidateTweet(id, tweetid);

                data.IsAcctValid = isAcctValid;
                data.IsTweetValid = isTweetValid;
            }
            catch (Exception e)
            {
                data.ErrorMessage = e.Message;
            }


            if (data.IsAcctValid && data.IsTweetValid)
            {
                try
                {
                    await _migrationService.MigrateAccountAsync(id, tweetid, handle, true);
                    data.MigrationSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    data.ErrorMessage = e.Message;
                }
            }

            return View("Index", data);
        }

        [HttpPost]
        [Route("/migration/{id}/{tweetid}/{handle}")]
        public async Task<IActionResult> RemoteMigrate(string id, string tweetid, string handle)
        {
            var isAcctValid = await _migrationService.ValidateFediverseAcctAsync(handle);
            var isTweetValid = _migrationService.ValidateTweet(id, tweetid);

            if (isAcctValid && isTweetValid)
            {
                await _migrationService.MigrateAccountAsync(id, tweetid, handle, false);
                return Ok();
            }

            return StatusCode(500);
        }
    }

    

    public class MigrationData
    {
        public string Acct { get; set; }
        
        public string FediverseAccount { get; set; }
        public string TweetId { get; set; }
        
        public string MigrationCode { get; set; }

        public bool IsTweetProvided { get; set; }
        public bool IsAcctProvided { get; set; }

        public bool IsTweetValid { get; set; }
        public bool IsAcctValid { get; set; }

        public string ErrorMessage { get; set; }
        public bool MigrationSuccess { get; set; }
    }
}
