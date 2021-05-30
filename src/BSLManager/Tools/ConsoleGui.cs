using System.Reflection;
using Terminal.Gui;

namespace BSLManager.Tools
{
    public static class ConsoleGui
    {
        public static void RefreshUI()
        {
            typeof(Application)
                .GetMethod("TerminalResized", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, null);
        }
    }
}