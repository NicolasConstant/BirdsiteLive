namespace BirdsiteLive.Models
{
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