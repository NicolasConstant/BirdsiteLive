using System.Collections.Generic;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Models;

namespace BirdsiteLive.DAL.Contracts
{
    public interface IFollowersDal
    {
        Task<Follower> GetFollowerAsync(string acct, string host);
        Task CreateFollowerAsync(string acct, string host, int[] followings, Dictionary<int, long> followingSyncStatus, string inboxUrl);
        Task<Follower[]> GetFollowersAsync(int followedUserId);
        Task UpdateFollowerAsync(int id, int[] followings, Dictionary<int, long> followingSyncStatus);
        Task DeleteFollowerAsync(int id);
        Task DeleteFollowerAsync(string acct, string host);
    }
}