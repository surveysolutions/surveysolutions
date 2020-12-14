using System;

namespace WB.Services.Export.Tests
{
    public static class TestConfig
    {
        public static string GetConnectionString()
        {
            return Environment.GetEnvironmentVariable("TEST_DB") ??
                   "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Qwerty1234;Database=export_service_tests;";
        }
    }
}
