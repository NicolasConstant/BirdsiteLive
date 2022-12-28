using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Moderation.Actions;

namespace BirdsiteLive.Moderation.Processors
{
    public interface ITwitterAccountModerationProcessor
    {
        Task ProcessAsync(ModerationTypeEnum type);
    }

    public class TwitterAccountModerationProcessor : ITwitterAccountModerationProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IModerationRepository _moderationRepository;
        private readonly IRemoveTwitterAccountAction _removeTwitterAccountAction;
        
        #region Ctor
        public TwitterAccountModerationProcessor(ITwitterUserDal twitterUserDal, IModerationRepository moderationRepository, IRemoveTwitterAccountAction removeTwitterAccountAction)
        {
            _twitterUserDal = twitterUserDal;
            _moderationRepository = moderationRepository;
            _removeTwitterAccountAction = removeTwitterAccountAction;
        }
        #endregion

        public async Task ProcessAsync(ModerationTypeEnum type)
        {
            if (type == ModerationTypeEnum.None) return;
            
            var twitterUsers = await _twitterUserDal.GetAllTwitterUsersAsync(false);

            foreach (var user in twitterUsers)
            {
                var userHandle = user.Acct.ToLowerInvariant().Trim();
                var status = _moderationRepository.CheckStatus(ModerationEntityTypeEnum.TwitterAccount, userHandle);

                if (type == ModerationTypeEnum.WhiteListing && status != ModeratedTypeEnum.WhiteListed ||
                    type == ModerationTypeEnum.BlackListing && status == ModeratedTypeEnum.BlackListed)
                    await _removeTwitterAccountAction.ProcessAsync(user);
            }
        }
    }
}