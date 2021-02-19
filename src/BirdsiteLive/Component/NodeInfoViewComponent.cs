using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Component
{
    public class NodeInfoViewComponent : ViewComponent
    {
        private readonly IModerationRepository _moderationRepository;

        #region Ctor
        public NodeInfoViewComponent(IModerationRepository moderationRepository)
        {
            _moderationRepository = moderationRepository;
        }
        #endregion

        public IViewComponentResult Invoke()
        {
            var followerPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.Follower);
            var twitterAccountPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.TwitterAccount);

            var viewModel = new NodeInfoViewModel
            {
                BlacklistingEnabled = followerPolicy == ModerationTypeEnum.BlackListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.BlackListing,
                WhitelistingEnabled = followerPolicy == ModerationTypeEnum.WhiteListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.WhiteListing,
                InstanceSaturation = 16,
            };
            
            viewModel = new NodeInfoViewModel
            {
                BlacklistingEnabled = false,
                WhitelistingEnabled = false,
                InstanceSaturation = 175
            };
            return View(viewModel);
        }
    }

    public class NodeInfoViewModel
    {
        public bool BlacklistingEnabled { get; set; }
        public bool WhitelistingEnabled { get; set; }
        public int InstanceSaturation { get; set; }
    }
}
