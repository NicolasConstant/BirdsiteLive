using System;
using System.Threading.Tasks;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Moderation.Processors;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive.Moderation
{
    public interface IModerationPipeline
    {
        Task ApplyModerationSettingsAsync();
    }

    public class ModerationPipeline : IModerationPipeline
    {
        private readonly IModerationRepository _moderationRepository;
        private readonly IFollowerModerationProcessor _followerModerationProcessor;
        private readonly ITwitterAccountModerationProcessor _twitterAccountModerationProcessor;

        private readonly ILogger<ModerationPipeline> _logger;

        #region Ctor
        public ModerationPipeline(IModerationRepository moderationRepository, IFollowerModerationProcessor followerModerationProcessor, ITwitterAccountModerationProcessor twitterAccountModerationProcessor, ILogger<ModerationPipeline> logger)
        {
            _moderationRepository = moderationRepository;
            _followerModerationProcessor = followerModerationProcessor;
            _twitterAccountModerationProcessor = twitterAccountModerationProcessor;
            _logger = logger;
        }
        #endregion

        public async Task ApplyModerationSettingsAsync()
        {
            try
            {
                await CheckFollowerModerationPolicyAsync();
                await CheckTwitterAccountModerationPolicyAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "ModerationPipeline execution failed.");
            }
        }

        private async Task CheckFollowerModerationPolicyAsync()
        {
            var followerPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.Follower);
            if (followerPolicy == ModerationTypeEnum.None) return;

            await _followerModerationProcessor.ProcessAsync(followerPolicy);
        }

        private async Task CheckTwitterAccountModerationPolicyAsync()
        {
            var twitterAccountPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.TwitterAccount);
            if (twitterAccountPolicy == ModerationTypeEnum.None) return;

            await _twitterAccountModerationProcessor.ProcessAsync(twitterAccountPolicy);
        }
    }
}
