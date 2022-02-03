namespace BirdsiteLive.Common.Settings
{
    public class InstanceSettings
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string AdminEmail { get; set; }
        public bool ResolveMentionsInProfiles { get; set; }
        public bool PublishReplies { get; set; }
        public int MaxUsersCapacity { get; set; }

        public string UnlistedTwitterAccounts { get; set; }
        public string SensitiveTwitterAccounts { get; set; }

        public int FailingTwitterUserCleanUpThreshold { get; set; }
        public int FailingFollowerCleanUpThreshold { get; set; } = -1;
    }
}
