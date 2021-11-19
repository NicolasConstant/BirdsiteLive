using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class RetrieveFollowersProcessor : IRetrieveFollowersProcessor
    {
        private readonly IFollowersDal _followersDal;

        #region Ctor
        public RetrieveFollowersProcessor(IFollowersDal followersDal)
        {
            _followersDal = followersDal;
        }
        #endregion

        public async Task<IEnumerable<UserWithDataToSync>> ProcessAsync(UserWithDataToSync[] userWithTweetsToSyncs, CancellationToken ct)
        {
            //TODO multithread this
            foreach (var user in userWithTweetsToSyncs)
            {
                var followers = await _followersDal.GetFollowersAsync(user.User.Id);
                user.Followers = followers;
            }

            return userWithTweetsToSyncs;
        }
    }
}