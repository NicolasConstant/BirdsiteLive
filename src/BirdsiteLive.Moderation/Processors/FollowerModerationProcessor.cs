using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Moderation.Actions;

namespace BirdsiteLive.Moderation.Processors
{
    public interface IFollowerModerationProcessor
    {
        Task ProcessAsync(ModerationTypeEnum type);
    }

    public class FollowerModerationProcessor : IFollowerModerationProcessor
    {
        private readonly IFollowersDal _followersDal;
        private readonly IModerationRepository _moderationRepository;
        private readonly IRemoveFollowerAction _removeFollowerAction;

        #region Ctor
        public FollowerModerationProcessor(IFollowersDal followersDal, IModerationRepository moderationRepository, IRemoveFollowerAction removeFollowerAction)
        {
            _followersDal = followersDal;
            _moderationRepository = moderationRepository;
            _removeFollowerAction = removeFollowerAction;
        }
        #endregion

        public async Task ProcessAsync(ModerationTypeEnum type)
        {
            if (type == ModerationTypeEnum.None) return;

            var followers = await _followersDal.GetAllFollowersAsync();

            foreach (var follower in followers)
            {
                var followerHandle = $"@{follower.Acct.Trim()}@{follower.Host.Trim()}".ToLowerInvariant();
                var status = _moderationRepository.CheckStatus(ModerationEntityTypeEnum.Follower, followerHandle);

                if (type == ModerationTypeEnum.WhiteListing && status != ModeratedTypeEnum.WhiteListed ||
                    type == ModerationTypeEnum.BlackListing && status == ModeratedTypeEnum.BlackListed)
                {
                    Console.WriteLine($"Remove {followerHandle}");
                    await _removeFollowerAction.ProcessAsync(follower);
                }
            }
        }
    }
}