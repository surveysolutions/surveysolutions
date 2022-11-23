using System;
using Microsoft.Extensions.Configuration;

namespace WB.Services.Export.Tests
{
    public static class TestConfig
    {
        public static string GetConnectionString()
        {
            return Environment.GetEnvironmentVariable("TEST_DB") ??
                   new ConfigurationBuilder()
                       .AddJsonFile($@"appsettings.json", true)
                       .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                       .Build()
                       .GetConnectionString("DefaultConnection");
        }
    }
}
