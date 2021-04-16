using System;
using System.Net.Http;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Common.Structs;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Settings;
using Lamar;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BSLManager
{
    public class Bootstrapper
    {
        private readonly DbSettings _dbSettings;
        private readonly InstanceSettings _instanceSettings;

        #region Ctor
        public Bootstrapper(DbSettings dbSettings, InstanceSettings instanceSettings)
        {
            _dbSettings = dbSettings;
            _instanceSettings = instanceSettings;
        }
        #endregion

        public Container Init()
        {
            var container = new Container(x =>
            {
                x.For<DbSettings>().Use(x => _dbSettings);

                x.For<InstanceSettings>().Use(x => _instanceSettings);

                if (string.Equals(_dbSettings.Type, DbTypes.Postgres, StringComparison.OrdinalIgnoreCase))
                {
                    var connString = $"Host={_dbSettings.Host};Username={_dbSettings.User};Password={_dbSettings.Password};Database={_dbSettings.Name}";
                    var postgresSettings = new PostgresSettings
                    {
                        ConnString = connString
                    };
                    x.For<PostgresSettings>().Use(x => postgresSettings);

                    x.For<ITwitterUserDal>().Use<TwitterUserPostgresDal>().Singleton();
                    x.For<IFollowersDal>().Use<FollowersPostgresDal>().Singleton();
                    x.For<IDbInitializerDal>().Use<DbInitializerPostgresDal>().Singleton();
                }
                else
                {
                    throw new NotImplementedException($"{_dbSettings.Type} is not supported");
                }

                var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
                x.For<IHttpClientFactory>().Use(_ => serviceProvider.GetService<IHttpClientFactory>());

                x.For(typeof(ILogger<>)).Use(typeof(DummyLogger<>));

                x.Scan(_ =>
                {
                    _.Assembly("BirdsiteLive.Twitter");
                    _.Assembly("BirdsiteLive.Domain");
                    _.Assembly("BirdsiteLive.DAL");
                    _.Assembly("BirdsiteLive.DAL.Postgres");
                    _.Assembly("BirdsiteLive.Moderation");

                    _.TheCallingAssembly();

                    _.WithDefaultConventions();

                    _.LookForRegistries();
                });
            });
            return container;
        }

        public class DummyLogger<T> : ILogger<T>
        {
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}