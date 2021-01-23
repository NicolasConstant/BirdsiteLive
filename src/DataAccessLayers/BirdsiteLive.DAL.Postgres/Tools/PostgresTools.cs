using System;
using System.Threading.Tasks;
using BirdsiteLive.DAL.Postgres.Settings;
using Npgsql;

namespace BirdsiteLive.DAL.Postgres.Tools
{
    public class PostgresTools
    {
        private readonly PostgresSettings _settings;

        #region Ctor
        public PostgresTools(PostgresSettings settings)
        {
            _settings = settings;
        }
        #endregion

        public async Task ExecuteRequestAsync(string request)
        {
            using (var conn = new NpgsqlConnection(_settings.ConnString))
            using (var cmd = new NpgsqlCommand(request, conn))
            {
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}