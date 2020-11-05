using System;
using Npgsql;

namespace WB.Tests.Integration
{
    public class TestsConfigurationManager
    {
        public static string ConnectionString
        {
            get
            {
                var cs = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=postgres;CommandTimeout=60";
                var dbHost = Environment.GetEnvironmentVariable("TEST_DATABASE_HOST");

                if (!string.IsNullOrWhiteSpace(dbHost))
                {
                    var npgBuilder = new NpgsqlConnectionStringBuilder(cs)
                    {
                        Host = dbHost
                    };

                    return npgBuilder.ConnectionString;
                }

                return cs;
            }
        }
    }
}
