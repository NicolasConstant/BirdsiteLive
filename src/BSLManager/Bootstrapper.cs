using System;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Common.Structs;
using BirdsiteLive.DAL.Contracts;
using BirdsiteLive.DAL.Postgres.DataAccessLayers;
using BirdsiteLive.DAL.Postgres.Settings;
using Lamar;

namespace BSLManager
{
    public class Bootstrapper
    {
        private readonly DbSettings _settings;

        #region Ctor
        public Bootstrapper(DbSettings settings)
        {
            _settings = settings;
        }
        #endregion

        public Container Init()
        {
            var container = new Container(x =>
            {
                if (string.Equals(_settings.Type, DbTypes.Postgres, StringComparison.OrdinalIgnoreCase))
                {
                    var connString = $"Host={_settings.Host};Username={_settings.User};Password={_settings.Password};Database={_settings.Name}";
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
                    throw new NotImplementedException($"{_settings.Type} is not supported");
                }

                x.Scan(_ =>
                {
                    //_.Assembly("BirdsiteLive.Twitter");
                    //_.Assembly("BirdsiteLive.Domain");
                    _.Assembly("BirdsiteLive.DAL");
                    _.Assembly("BirdsiteLive.DAL.Postgres");
                    //_.Assembly("BirdsiteLive.Moderation");
                    //_.Assembly("BirdsiteLive.Pipeline");
                    _.TheCallingAssembly();

                    _.WithDefaultConventions();

                    _.LookForRegistries();
                });
            });
            return container;
        }
    }
}