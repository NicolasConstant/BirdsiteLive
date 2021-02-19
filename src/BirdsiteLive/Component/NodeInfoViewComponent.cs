using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Domain.Repository;
using BirdsiteLive.Statistics.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace BirdsiteLive.Component
{
    public class NodeInfoViewComponent : ViewComponent
    {
        private readonly IModerationRepository _moderationRepository;
        private readonly ITwitterStatisticsHandler _twitterStatisticsHandler;
        private readonly ITwitterUserDal _twitterUserDal;

        #region Ctor
        public NodeInfoViewComponent(IModerationRepository moderationRepository, ITwitterStatisticsHandler twitterStatisticsHandler, ITwitterUserDal twitterUserDal)
        {
            _moderationRepository = moderationRepository;
            _twitterStatisticsHandler = twitterStatisticsHandler;
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var followerPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.Follower);
            var twitterAccountPolicy = _moderationRepository.GetModerationType(ModerationEntityTypeEnum.TwitterAccount);

            var statistics = await GetStatisticsAsync();

            var viewModel = new NodeInfoViewModel
            {
                BlacklistingEnabled = followerPolicy == ModerationTypeEnum.BlackListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.BlackListing,
                WhitelistingEnabled = followerPolicy == ModerationTypeEnum.WhiteListing ||
                                      twitterAccountPolicy == ModerationTypeEnum.WhiteListing,
                InstanceSaturation = statistics.Saturation
            };
            
            //viewModel = new NodeInfoViewModel
            //{
            //    BlacklistingEnabled = false,
            //    WhitelistingEnabled = false,
            //    InstanceSaturation = 175
            //};
            return View(viewModel);
        }

        private static CachedStatistics _cachedStatistics;
        private async Task<CachedStatistics> GetStatisticsAsync() {
            if (_cachedStatistics == null ||
                (DateTime.UtcNow - _cachedStatistics.RefreshedTime).TotalMinutes > 15)
            {
                var twitterUserMax = _twitterStatisticsHandler.GetStatistics().UserCallsMax;
                var twitterUserCount = await _twitterUserDal.GetTwitterUsersCountAsync();
                var saturation = (int)((double)twitterUserCount / twitterUserMax * 100);

                _cachedStatistics = new CachedStatistics
                {
                    RefreshedTime = DateTime.UtcNow,
                    Saturation = saturation
                };
            }

            return _cachedStatistics;
        }

        class CachedStatistics
        {
            public DateTime RefreshedTime { get; set; }
            public int Saturation { get; set; }
        }

    }

    public class NodeInfoViewModel
    {
        public bool BlacklistingEnabled { get; set; }
        public bool WhitelistingEnabled { get; set; }
        public int InstanceSaturation { get; set; }
    }
}
