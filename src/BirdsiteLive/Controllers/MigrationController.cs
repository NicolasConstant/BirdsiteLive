using System;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Npgsql.TypeHandlers;
using BirdsiteLive.Domain;
using BirdsiteLive.Domain.Enum;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Models;
using System.Reflection.Metadata;

namespace BirdsiteLive.Controllers
{
    public class MigrationController : Controller
    {
        private readonly MigrationService _migrationService;
        private readonly ITwitterUserDal _twitterUserDal;
        
        #region Ctor
        public MigrationController(MigrationService migrationService, ITwitterUserDal twitterUserDal)
        {
            _migrationService = migrationService;
            _twitterUserDal = twitterUserDal;
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

            //Verify can be migrated 
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(id);
            if (twitterAccount != null && twitterAccount.Deleted)
            {
                data.ErrorMessage = "This account has been deleted, it can't be migrated";
                return View("Index", data);
            }
            if (twitterAccount != null &&
                (!string.IsNullOrWhiteSpace(twitterAccount.MovedTo)
                 || !string.IsNullOrWhiteSpace(twitterAccount.MovedToAcct)))
            {
                data.ErrorMessage = "This account has been moved already, it can't be migrated again";
                return View("Index", data);
            }

            // Start migration
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
                    _migrationService.TriggerRemoteMigrationAsync(id, tweetid, handle);
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
            var deletionCode = _migrationService.GetDeletionCode(id);

            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = deletionCode,

                IsTweetProvided = !string.IsNullOrWhiteSpace(tweetid),

                TweetId = tweetid
            };

            //Verify can be deleted 
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(id);
            if (twitterAccount != null && twitterAccount.Deleted)
            {
                data.ErrorMessage = "This account has been deleted, it can't be deleted again";
                return View("Delete", data);
            }

            // Start deletion
            try
            {
                var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Deletion);
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
                    _migrationService.TriggerRemoteDeleteAsync(id, tweetid);
                    data.MigrationSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    data.ErrorMessage = e.Message;
                }
            }

            return View("Delete", data);
        }

        [HttpPost]
        [Route("/migration/move/{id}/{tweetid}/{handle}")]
        public async Task<IActionResult> RemoteMigrateMove(string id, string tweetid, string handle)
        {
            //Check inputs 
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(tweetid) ||
                string.IsNullOrWhiteSpace(handle))
                return StatusCode(422);

            //Verify can be migrated 
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(id);
            if (twitterAccount != null && (twitterAccount.Deleted 
                || !string.IsNullOrWhiteSpace(twitterAccount.MovedTo) 
                || !string.IsNullOrWhiteSpace(twitterAccount.MovedToAcct)))
                return Ok();

            // Start migration
            var fediverseUserValidation = await _migrationService.ValidateFediverseAcctAsync(handle);
            var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Migration);

            if (fediverseUserValidation.IsValid && isTweetValid)
            {
                await _migrationService.MigrateAccountAsync(fediverseUserValidation, id);
                return Ok();
            }

            return StatusCode(400);
        }

        [HttpPost]
        [Route("/migration/delete/{id}/{tweetid}")]
        public async Task<IActionResult> RemoteMigrateDelete(string id, string tweetid)
        {
            //Check inputs 
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(tweetid))
                return StatusCode(422);

            //Verify can be deleted 
            var twitterAccount = await _twitterUserDal.GetTwitterUserAsync(id);
            if (twitterAccount != null && twitterAccount.Deleted) return Ok();

            // Start deletion
            var isTweetValid = _migrationService.ValidateTweet(id, tweetid, MigrationTypeEnum.Deletion);

            if (isTweetValid)
            {
                await _migrationService.DeleteAccountAsync(id);
                return Ok();
            }

            return StatusCode(400);
        }
    }
}
