using System;
using System.Linq;
using BirdsiteLive.Twitter;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BirdsiteLive.Domain
{
    public class MigrationService
    {
        private readonly ITwitterTweetsService _twitterTweetsService;

        #region Ctor
        public MigrationService(ITwitterTweetsService twitterTweetsService)
        {
            _twitterTweetsService = twitterTweetsService;
        }
        #endregion

        public string GetMigrationCode(string acct)
        {
            var hash = GetHashString(acct);
            return $"[[BirdsiteLIVE-MigrationCode|{hash.Substring(0, 10)}]]";
        }

        public bool ValidateTweet(string acct, string tweetId)
        {
            var code = GetMigrationCode(acct);

            var castedTweetId = ExtractedTweetId(tweetId);
            var tweet = _twitterTweetsService.GetTweet(castedTweetId);

            if (tweet == null) throw new Exception("Tweet not found");
            if (!tweet.MessageContent.Contains(code)) throw new Exception("Tweet don't have migration code");

            return true;
        }

        private long ExtractedTweetId(string tweetId)
        {
            long castedId;
            if (long.TryParse(tweetId, out castedId))
                return castedId;

            var urlPart = tweetId.Split('/').LastOrDefault();
            if (long.TryParse(urlPart, out castedId))
                return castedId;

            throw new ArgumentException("Unvalid Tweet ID");
        }

        public async Task<bool> ValidateFediverseAcctAsync(string fediverseAcct)
        {
            return true;
        }

        public async Task MigrateAccountAsync(string acct, string tweetId, string fediverseAcct, bool triggerRemoteMigration)
        {
        }

        private byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}