using BirdsiteLive.Common.Settings;
using System;

namespace BirdsiteLive.Pipeline.Processors.TweetsCleanUp.Base
{
    public class RetentionBase
    {
        protected int GetRetentionTime(InstanceSettings settings)
        {
            var retentionTime = Math.Abs(settings.MaxTweetRetention);
            if (retentionTime < 1) retentionTime = 1;
            if (retentionTime > 90) retentionTime = 90;
            return retentionTime;
        }
    }
}