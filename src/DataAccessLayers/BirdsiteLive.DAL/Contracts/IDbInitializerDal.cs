using System;
using System.Reflection;
using System.Threading.Tasks;

namespace BirdsiteLive.DAL.Contracts
{
    public interface IDbInitializerDal
    { 
        Task<Version> GetCurrentDbVersionAsync();
        Version GetMandatoryDbVersion();
        Tuple<Version, Version>[] GetMigrationPatterns();
        Task<Version> MigrateDbAsync(Version from, Version to);
        Task<Version> InitDbAsync();
    }
}