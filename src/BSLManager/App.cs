using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Models;
using BirdsiteLive.Moderation.Actions;
using Terminal.Gui;

namespace BSLManager
{
    public class App
    {
        private readonly IFollowersDal _followersDal;
        private readonly IRemoveFollowerAction _removeFollowerAction;

        private List<string> _displayableUserList = new List<string>();
        private List<Follower> _sourceUserList = new List<Follower>();

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

            var list = new ListView(_displayableUserList)
            {
                X = 1,
                Y = 2,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            list.KeyDown += _ =>
            {
                if (_.KeyEvent.Key == Key.Enter)
                {
                    var el = list.SelectedItem;

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

                    var name = new Label($"User: {_displayableUserList[el]}")
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
                        DeleteAndRemoveUser(el);
                    }
                }
            };

            // Add some controls, 
            win.Add(
                new Label(1, 0, "Listing followers"),
                list
            );

            Application.Run();
        }

        private void DeleteAndRemoveUser(int el)
        {
            Application.MainLoop.Invoke(async () =>
            {
                var userToDelete = _sourceUserList[el];

                await _removeFollowerAction.ProcessAsync(userToDelete);

                _sourceUserList.RemoveAt(el);
                _displayableUserList.RemoveAt(el);
                RefreshUI();
            });
        }

        private void RetrieveUserList()
        {
            Application.MainLoop.Invoke(async () =>
            {
                var followers = await _followersDal.GetAllFollowersAsync();
                _sourceUserList = followers.OrderByDescending(x => x.Followings.Count).ToList();

                _displayableUserList.Clear();
                foreach (var follower in _sourceUserList)
                {
                    _displayableUserList.Add($"@{follower.Acct}@{follower.Host}     {follower.Followings.Count}");
                }

                RefreshUI();
            });
        }

        private void RefreshUI()
        {
            typeof(Application).GetMethod("TerminalResized", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, null);
        }
    }
}