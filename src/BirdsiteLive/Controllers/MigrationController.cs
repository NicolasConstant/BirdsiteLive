using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql.TypeHandlers;
using BirdsiteLive.Domain;
using BirdsiteLive.Domain.Enum;

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
        [Route("/migration/move/{id}")]
        public IActionResult IndexMove(string id)
        {
            var migrationCode = _migrationService.GetMigrationCode(id);
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode
            };

            return View("Index", data);
        }

        [HttpGet]
        [Route("/migration/delete/{id}")]
        public IActionResult IndexDelete(string id)
        {
            var migrationCode = _migrationService.GetDeletionCode(id);
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode
            };

            return View("Delete", data);
        }

        [HttpPost]
        [Route("/migration/move/{id}")]
        public async Task<IActionResult> MigrateMove(string id, string tweetid, string handle)
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

            ValidatedFediverseUser fediverseUserValidation = null;

            try
            {
                fediverseUserValidation = await _migrationService.ValidateFediverseAcctAsync(handle);
                var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Migration);

                data.IsAcctValid = fediverseUserValidation.IsValid;
                data.IsTweetValid = isTweetValid;
            }
            catch (Exception e)
            {
                data.ErrorMessage = e.Message;
            }
            
            if (data.IsAcctValid && data.IsTweetValid && fediverseUserValidation != null)
            {
                try
                {
                    await _migrationService.MigrateAccountAsync(fediverseUserValidation, id);
                    await _migrationService.TriggerRemoteMigrationAsync(id, tweetid, handle);
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
        [Route("/migration/delete/{id}")]
        public async Task<IActionResult> MigrateDelete(string id, string tweetid)
        {
            var migrationCode = _migrationService.GetMigrationCode(id);

            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode,

                IsTweetProvided = !string.IsNullOrWhiteSpace(tweetid),

                TweetId = tweetid
            };

            try
            {
                var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Migration);
                data.IsTweetValid = isTweetValid;
            }
            catch (Exception e)
            {
                data.ErrorMessage = e.Message;
            }

            if (data.IsTweetValid)
            {
                try
                {
                    await _migrationService.DeleteAccountAsync(id);
                    await _migrationService.TriggerRemoteDeleteAsync(id, tweetid);
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
        [Route("/migration/move/{id}/{tweetid}/{handle}")]
        public async Task<IActionResult> RemoteMigrateMove(string id, string tweetid, string handle)
        {
            var fediverseUserValidation = await _migrationService.ValidateFediverseAcctAsync(handle);
            var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Deletion);

            if (fediverseUserValidation.IsValid && isTweetValid)
            {
                await _migrationService.MigrateAccountAsync(fediverseUserValidation, id);
                return Ok();
            }

            return StatusCode(500);
        }

        [HttpPost]
        [Route("/migration/delete/{id}/{tweetid}/{handle}")]
        public async Task<IActionResult> RemoteDeleteMove(string id, string tweetid, string handle)
        {
            throw new NotImplementedException();
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
