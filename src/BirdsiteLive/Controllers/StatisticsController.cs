using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Statistics.Domain;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IFollowersDal _followersDal;
        private readonly ITwitterStatisticsHandler _twitterStatistics;

        #region Ctor
        public StatisticsController(ITwitterUserDal twitterUserDal, IFollowersDal followersDal, ITwitterStatisticsHandler twitterStatistics)
        {
            _twitterUserDal = twitterUserDal;
            _followersDal = followersDal;
            _twitterStatistics = twitterStatistics;
        }
        #endregion

        public async Task<IActionResult> Index()
        {
            var stats = new Models.StatisticsModels.Statistics
            {
                FollowersCount = await _followersDal.GetFollowersCountAsync(),
                TwitterUserCount = await _twitterUserDal.GetTwitterUsersCountAsync(),
                TwitterStatistics = _twitterStatistics.GetStatistics()
            };
            return View(stats);
        }
    }
}
