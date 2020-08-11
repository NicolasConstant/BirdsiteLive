using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.DAL.Contracts
{
    public interface IFollowersDal
    {
        Task<Follower> GetFollowerAsync(string acct, string host);
        Task CreateFollowerAsync(string acct, string host, string inboxRoute, string sharedInboxRoute, int[] followings = null,
            Dictionary<int, long> followingSyncStatus = null);
        Task<Follower[]> GetFollowersAsync(int followedUserId);
        Task UpdateFollowerAsync(Follower follower);
        Task DeleteFollowerAsync(int id);
        Task DeleteFollowerAsync(string acct, string host);
    }
}