using System;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Contracts;

namespace BirdsiteLive.DAL
{
    public interface IDatabaseInitializer
    {
        Task InitAndMigrateDbAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly IDbInitializerDal _dbInitializerDal;

        #region Ctor
        public DatabaseInitializer(IDbInitializerDal dbInitializerDal)
        {
            _dbInitializerDal = dbInitializerDal;
        }
        #endregion

        public async Task InitAndMigrateDbAsync()
        {
            var currentVersion = await _dbInitializerDal.GetCurrentDbVersionAsync();
            var mandatoryVersion = _dbInitializerDal.GetMandatoryDbVersion();

            if (currentVersion == mandatoryVersion) return;

            // Init Db
            if (currentVersion == null)
                currentVersion = await _dbInitializerDal.InitDbAsync();

            // Migrate Db
            var migrationPatterns = _dbInitializerDal.GetMigrationPatterns();
            while (migrationPatterns.Any(x => x.Item1 == currentVersion))
            {
                var migration = migrationPatterns.First(x => x.Item1 == currentVersion);
                currentVersion = await _dbInitializerDal.MigrateDbAsync(migration.Item1, migration.Item2);
            }

            if (currentVersion != mandatoryVersion) throw new Exception("Migrating DB failed");
        }
    }
}