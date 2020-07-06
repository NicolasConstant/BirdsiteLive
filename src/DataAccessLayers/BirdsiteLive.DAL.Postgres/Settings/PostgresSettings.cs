namespace BirdsiteLive.DAL.Postgres.Settings
{
    public class PostgresSettings
    {
        public string ConnString { get; set; }

        public string DbVersionTableName { get; set; } = "db-version";
        public string TwitterUserTableName { get; set; } = "twitter-users";
        public string FollowersTableName { get; set; } = "followers";
        public string CachedTweetsTableName { get; set; } = "cached-tweets";
    }
}