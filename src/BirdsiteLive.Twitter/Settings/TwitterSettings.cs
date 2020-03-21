namespace BirdsiteLive.Twitter.Settings
{
    public class TwitterSettings
    {
        #region Ctor
        public TwitterSettings()
        {
            
        }

        public TwitterSettings(string apiKey)
        {
            ApiKey = apiKey;
        }
        #endregion

        public string ApiKey { get; set; }
    }
}