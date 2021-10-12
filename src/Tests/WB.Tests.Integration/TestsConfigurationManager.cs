using Microsoft.Extensions.Configuration;

namespace WB.Tests.Integration
{
    public static class TestsConfigurationManager
    {
        private static readonly IConfigurationRoot Config;
        
        static TestsConfigurationManager()
        {
            Config = new ConfigurationBuilder().AddIniFile("appsettings.tests.integration.ini").Build();
        }

        public static string ConnectionString => Config["ConnectionStrings:TestConnection"];
    }
}
