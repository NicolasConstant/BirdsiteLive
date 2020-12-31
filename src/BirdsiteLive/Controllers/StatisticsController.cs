using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Models.StatisticsModels;
using Microsoft.AspNetCore.Mvc;

namespace BirdsiteLive.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly ITwitterUserDal _twitterUserDal;
        private readonly IFollowersDal _followersDal;

        #region Ctor
        public StatisticsController(ITwitterUserDal twitterUserDal, IFollowersDal followersDal)
        {
            _twitterUserDal = twitterUserDal;
            _followersDal = followersDal;
        }
        #endregion

        public async Task<IActionResult> Index()
        {
            var stats = new Statistics
            {
                FollowersCount = await _followersDal.GetFollowersCountAsync(),
                TwitterUserCount = await _twitterUserDal.GetTwitterUsersCountAsync()
            };
            return View(stats);
        }
    }
}
