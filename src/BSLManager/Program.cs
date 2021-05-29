using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.DAL.Contracts;
using BSLManager.Tools;
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

            var settingsManager = new SettingsManager();
            var settings = settingsManager.GetSettings();

            //var builder = new ConfigurationBuilder()
            //    .AddEnvironmentVariables();
            //var configuration = builder.Build();

            //var dbSettings = configuration.GetSection("Db").Get<DbSettings>();
            //var instanceSettings = configuration.GetSection("Instance").Get<InstanceSettings>();

            var bootstrapper = new Bootstrapper(settings.dbSettings, settings.instanceSettings);
            var container = bootstrapper.Init();

            var app = container.GetInstance<App>();
            app.Run();
        }
    }
}
