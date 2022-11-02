using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Npgsql.TypeHandlers;

namespace BirdsiteLive.Controllers
{
    public class MigrationController : Controller
    {
        [HttpGet]
        [Route("/migration/{id}")]
        public IActionResult Index(string id)
        {
            var migrationCode = GetMigrationCode(id);
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode
            };

            return View(data);
        }

        [HttpPost]
        [Route("/migration/{id}")]
        public IActionResult Migrate(string id, string tweetid, string handle)
        {
            var migrationCode = GetMigrationCode(id);
            var data = new MigrationData()
            {
                Acct = id,
                MigrationCode = migrationCode,

                IsAcctProvided = !string.IsNullOrWhiteSpace(handle),
                IsTweetProvided = !string.IsNullOrWhiteSpace(tweetid),

                TweetId = tweetid,
                FediverseAccount = handle
            };

            return View("Index", data);
        }

        public byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public string GetMigrationCode(string acct)
        {
            var hash = GetHashString(acct);
            return $"[[BirdsiteLIVE-MigrationCode|{hash.Substring(0, 10)}]]";
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
        
    }
}
