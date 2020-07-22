using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.Pipeline.Contracts;
using BirdsiteLive.Pipeline.Models;

namespace BirdsiteLive.Pipeline.Processors
{
    public class SaveProgressionProcessor : ISaveProgressionProcessor
    {
        private readonly ITwitterUserDal _twitterUserDal;
        
        #region Ctor
        public SaveProgressionProcessor(ITwitterUserDal twitterUserDal)
        {
            _twitterUserDal = twitterUserDal;
        }
        #endregion

        public async Task ProcessAsync(UserWithTweetsToSync userWithTweetsToSync, CancellationToken ct)
        {
            var userId = userWithTweetsToSync.User.Id;
            var lastPostedTweet = userWithTweetsToSync.Tweets.Select(x => x.Id).Max();
            var minimumSync = userWithTweetsToSync.Followers.Select(x => x.FollowingsSyncStatus[userId]).Min();
            await _twitterUserDal.UpdateTwitterUserAsync(userId, lastPostedTweet, minimumSync);
        }
    }
}