using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using Microsoft.Extensions.Configuration;
using NStack;
using Terminal.Gui;

namespace BSLManager
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Default;

            var settings = GetSettings();

            var bootstrapper = new Bootstrapper(settings);
            var container = bootstrapper.Init();

            var app = container.GetInstance<App>();
            app.Run();
        }

        private static DbSettings GetSettings()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            var dbSettings = configuration.GetSection("Db").Get<DbSettings>();
            return dbSettings;
        }
    }
}
