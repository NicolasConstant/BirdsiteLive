using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NStack;
using Terminal.Gui;

namespace BSLManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Default;

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
            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_File", new MenuItem [] {
                    new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
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
            
            var listData = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                listData.Add($"@User{i}@Instance.tld        {i*3}");
            }

            var list = new ListView(listData)
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

                    var name = new Label($"User: {listData[el]}")
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
                        listData.RemoveAt(el);
                        typeof(Application).GetMethod("TerminalResized", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
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
    }
}
