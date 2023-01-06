using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Services;
using BirdsiteLive.Statistics.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace BirdsiteLive.Component
{
    public class NodeInfoViewComponent : ViewComponent
    {
        private readonly IModerationRepository _moderationRepository;
        private readonly ICachedStatisticsService _cachedStatisticsService;
        
        #region Ctor
        public NodeInfoViewComponent(IModerationRepository moderationRepository, ICachedStatisticsService cachedStatisticsService)
        {
            _moderationRepository = moderationRepository;
            _cachedStatisticsService = cachedStatisticsService;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var followerPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.Follower);
            var twitterAccountPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.TwitterAccount);

            var statistics = await _cachedStatisticsService.GetStatisticsAsync();

            var viewModel = new NodeInfoViewModel
            {
                BlocklistingEnabled = followerPolicy == ModerationTypeEnum.BlockListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.BlockListing,
                AllowlistingEnabled = followerPolicy == ModerationTypeEnum.AllowListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.AllowListing,
                InstanceSaturation = statistics.Saturation
            };
            
            //viewModel = new NodeInfoViewModel
            //{
            //    BlocklistingEnabled = false,
            //    AllowlistingEnabled = false,
            //    InstanceSaturation = 175
            //};
            return View(viewModel);
        }
    }

    public class NodeInfoViewModel
    {
        public bool BlocklistingEnabled { get; set; }
        public bool AllowlistingEnabled { get; set; }
        public int InstanceSaturation { get; set; }
    }
}
