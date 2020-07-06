using BirdsiteLive.DAL.Postgres.Settings;
using Npgsql;

namespace BirdsiteLive.DAL.Postgres.DataAccessLayers.Base
{
    public class PostgresBase
    {
        protected readonly PostgresSettings _settings;

        #region Ctor
        protected PostgresBase(PostgresSettings settings)
        {
            _settings = settings;
        }
        #endregion

        protected NpgsqlConnection Connection
        {
            get
            {
                return new NpgsqlConnection(_settings.ConnString);
            }
        }
    }
}