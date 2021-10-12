using Microsoft.Extensions.Configuration;

namespace WB.Tests.Integration
{
    public static class TestsConfigurationManager
    {
        private static readonly IConfigurationRoot Config;
        
        static TestsConfigurationManager()
        {
            Config = new ConfigurationBuilder().AddIniFile("appsettings.test.designer.ini").Build();
        }

        public static string TestConnectionStringFormat => Config["ConnectionSettings:TestConnectionStringFormat"];
    }
}
