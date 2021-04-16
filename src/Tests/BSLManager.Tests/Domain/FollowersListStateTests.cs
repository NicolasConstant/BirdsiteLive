using System.Collections.Generic;
using System.Linq;
using BirdsiteLive.DAL.Models;
using BSLManager.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BSLManager.Tests
{
    [TestClass]
    public class FollowersListStateTests
    {
        [TestMethod]
        public void FilterBy()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                }, 
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers);

            state.FilterBy("test");
            
            #region Validate
            Assert.AreEqual(2, state.GetDisplayableList().Count);
            #endregion
        }

        [TestMethod]
        public void FilterBy_GetElement()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers);

            state.FilterBy("test");
            var el = state.GetElementAt(1);

            #region Validate
            Assert.AreEqual(followers[1].Id, el.Id);
            #endregion
        }

        [TestMethod]
        public void GetElement()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers);

            var el = state.GetElementAt(2);

            #region Validate
            Assert.AreEqual(followers[2].Id, el.Id);
            #endregion
        }

        [TestMethod]
        public void FilterBy_RemoveAt()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers.ToList());

            state.FilterBy("test");
            state.RemoveAt(1);

            var list = state.GetDisplayableList();

            #region Validate
            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list[0].Contains("@test@host1"));
            #endregion
        }

        [TestMethod]
        public void RemoveAt()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers.ToList());

            state.RemoveAt(1);

            var list = state.GetDisplayableList();

            #region Validate
            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list[0].Contains("@test@host1"));
            Assert.IsFalse(list[1].Contains("@test@host2"));
            #endregion
        }

        [TestMethod]
        public void FilterBy_ResetFilter()
        {
            #region Stub
            var followers = new List<Follower>
            {
                new Follower
                {
                    Id = 0,
                    Acct = "test",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 1,
                    Acct = "test",
                    Host = "host2",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 2,
                    Acct = "user1",
                    Host = "host1",
                    Followings = new List<int>()
                },
                new Follower
                {
                    Id = 3,
                    Acct = "user2",
                    Host = "host1",
                    Followings = new List<int>()
                }
            };
            #endregion

            var state = new FollowersListState();
            state.Load(followers.ToList());
            
            #region Validate
            state.FilterBy("data");
            var list = state.GetDisplayableList();
            Assert.AreEqual(0, list.Count);

            state.FilterBy(string.Empty);
            list = state.GetDisplayableList();
            Assert.AreEqual(4, list.Count);
            #endregion
        }
    }
}
