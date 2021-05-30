using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using BSLManager.Domain;
using BSLManager.Tools;
using Terminal.Gui;

namespace BSLManager
{
    public class App
    {
        private readonly IFollowersDal _followersDal;
        private readonly IRemoveFollowerAction _removeFollowerAction;

        private readonly FollowersListState _state = new FollowersListState();

        #region Ctor
        public App(IFollowersDal followersDal, IRemoveFollowerAction removeFollowerAction)
        {
            _followersDal = followersDal;
            _removeFollowerAction = removeFollowerAction;
        }
        #endregion

        public void Run()
        {
            Application.Init();
            var top = Application.Top;

            // Creates the top-level window to show
            var win = new Window("BSL Manager")
            {
                X = 0,
                Y = 1, // Leave one row for the toplevel menu

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            top.Add(win);

            // Creates a menubar, the item "New" has a help menu.
            var menu = new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("_File", new MenuItem[]
                {
                    new MenuItem("_Quit", "", () =>
                    {
                        if (Quit()) top.Running = false;
                    })
                }),
                //new MenuBarItem ("_Edit", new MenuItem [] {
                //    new MenuItem ("_Copy", "", null),
                //    new MenuItem ("C_ut", "", null),
                //    new MenuItem ("_Paste", "", null)
                //})
            });
            top.Add(menu);

            static bool Quit()
            {
                var n = MessageBox.Query(50, 7, "Quit BSL Manager", "Are you sure you want to quit?", "Yes", "No");
                return n == 0;
            }
            
            RetrieveUserList();

            var list = new ListView(_state.GetDisplayableList())
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            list.KeyDown += _ =>
            {
                if (_.KeyEvent.Key == Key.Enter)
                {
                    OpenFollowerDialog(list.SelectedItem);
                } 
                else if (_.KeyEvent.Key == Key.Delete
                           || _.KeyEvent.Key == Key.DeleteChar
                           || _.KeyEvent.Key == Key.Backspace
                           || _.KeyEvent.Key == Key.D)
                {
                    OpenDeleteDialog(list.SelectedItem);
                }
            };

            var listingFollowersLabel = new Label(1, 0, "Listing followers");
            var filterLabel = new Label("Filter: ") { X = 1, Y = 1 };
            var filterText = new TextField("")
            {
                X = Pos.Right(filterLabel),
                Y = 1,
                Width = 40
            };

            filterText.KeyDown += _ =>
            {
                var text = filterText.Text.ToString();
                if (_.KeyEvent.Key == Key.Enter && !string.IsNullOrWhiteSpace(text))
                {
                    _state.FilterBy(text);
                    ConsoleGui.RefreshUI();
                }
            };

            win.Add(
                listingFollowersLabel,
                filterLabel,
                filterText,
                list
            );

            Application.Run();
        }

        private void OpenFollowerDialog(int selectedIndex)
        {
            var close = new Button(3, 14, "Close");
            close.Clicked += () => Application.RequestStop();

            var dialog = new Dialog("Info", 60, 18, close);

            var follower = _state.GetElementAt(selectedIndex);

            var name = new Label($"User: @{follower.Acct}@{follower.Host}")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            var following = new Label($"Following Count: {follower.Followings.Count}")
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Height = 1
            };
            var inbox = new Label($"Inbox: {follower.InboxRoute}")
            {
                X = 1,
                Y = 4,
                Width = Dim.Fill(),
                Height = 1
            };
            var sharedInbox = new Label($"Shared Inbox: {follower.SharedInboxRoute}")
            {
                X = 1,
                Y = 5,
                Width = Dim.Fill(),
                Height = 1
            };

            dialog.Add(name);
            dialog.Add(following);
            dialog.Add(inbox);
            dialog.Add(sharedInbox);
            dialog.Add(close);
            Application.Run(dialog);
        }

        private void OpenDeleteDialog(int selectedIndex)
        {
            bool okpressed = false;
            var ok = new Button(10, 14, "Yes");
            ok.Clicked += () =>
            {
                Application.RequestStop();
                okpressed = true;
            };

            var cancel = new Button(3, 14, "No");
            cancel.Clicked += () => Application.RequestStop();

            var dialog = new Dialog("Delete", 60, 18, cancel, ok);

            var follower = _state.GetElementAt(selectedIndex);
            var name = new Label($"User: @{follower.Acct}@{follower.Host}")
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            var entry = new Label("Delete user and remove all their followings?")
            {
                X = 1,
                Y = 3,
                Width = Dim.Fill(),
                Height = 1
            };
            dialog.Add(name);
            dialog.Add(entry);
            Application.Run(dialog);

            if (okpressed)
            {
                DeleteAndRemoveUser(selectedIndex);
            }
        }

        private void DeleteAndRemoveUser(int el)
        {
            Application.MainLoop.Invoke(async () =>
            {
                try
                {
                    var userToDelete = _state.GetElementAt(el);
                    
                    BasicLogger.Log($"Delete {userToDelete.Acct}@{userToDelete.Host}");
                    await _removeFollowerAction.ProcessAsync(userToDelete);
                    BasicLogger.Log($"Remove user from list");
                    _state.RemoveAt(el);
                }
                catch (Exception e)
                {
                    BasicLogger.Log(e.Message);
                }

                ConsoleGui.RefreshUI();
            });
        }

        private void RetrieveUserList()
        {
            Application.MainLoop.Invoke(async () =>
            {
                var followers = await _followersDal.GetAllFollowersAsync();
                _state.Load(followers.ToList());
                ConsoleGui.RefreshUI();
            });
        }
    }
}