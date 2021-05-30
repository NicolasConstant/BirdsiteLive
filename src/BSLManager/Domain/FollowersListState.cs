using System.Collections.Generic;
using System.Linq;
using BirdsiteLive.DAL.Models;

namespace BSLManager.Domain
{
    public class FollowersListState
    {
        private readonly List<string> _filteredDisplayableUserList = new List<string>();

        private List<Follower> _sourceUserList = new List<Follower>();
        private List<Follower> _filteredSourceUserList = new List<Follower>();
        
        public void Load(List<Follower> followers)
        {
            _sourceUserList = followers.OrderByDescending(x => x.Followings.Count).ToList();
           
            ResetLists();
        }

        private void ResetLists()
        {
            _filteredSourceUserList = _sourceUserList.ToList();

            _filteredDisplayableUserList.Clear();

            foreach (var follower in _sourceUserList)
            {
                var displayedUser = $"{GetFullHandle(follower)}     ({follower.Followings.Count})";
                _filteredDisplayableUserList.Add(displayedUser);
            }
        }

        public List<string> GetDisplayableList()
        {
            return _filteredDisplayableUserList;
        }

        public void FilterBy(string pattern)
        {
            ResetLists();

            if (!string.IsNullOrWhiteSpace(pattern))
            {
                var elToRemove = _filteredSourceUserList
                    .Where(x => !GetFullHandle(x).Contains(pattern))
                    .Select(x => x)
                    .ToList();

                foreach (var el in elToRemove)
                {
                    _filteredSourceUserList.Remove(el);
                    
                    var dElToRemove = _filteredDisplayableUserList.First(x => x.Contains(GetFullHandle(el)));
                    _filteredDisplayableUserList.Remove(dElToRemove);
                }
            }
        }

        private string GetFullHandle(Follower follower)
        {
            return $"@{follower.Acct}@{follower.Host}";
        }

        public void RemoveAt(int index)
        {
            var displayableUser = _filteredDisplayableUserList[index];
            var sourceUser = _filteredSourceUserList[index];

            _filteredDisplayableUserList.Remove(displayableUser);

            _filteredSourceUserList.Remove(sourceUser);
            _sourceUserList.Remove(sourceUser);
        }

        public Follower GetElementAt(int index)
        {
            return _filteredSourceUserList[index];
        }
    }
}