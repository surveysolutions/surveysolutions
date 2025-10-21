﻿using System;
using Microsoft.Extensions.Configuration;

namespace WB.Tests.Integration
{
    public static class TestsConfigurationManager
    {
        private static readonly IConfigurationRoot Config;
        
        static TestsConfigurationManager()
        {
            Config = new ConfigurationBuilder()
                .AddIniFile("appsettings.ini")
                .AddIniFile("appsettings.cloud.ini", true)
                .AddIniFile($"appsettings.{Environment.MachineName.ToLower()}.ini", true)
                .Build();
        }

        public static string ConnectionString => Config["ConnectionStrings:TestConnection"];
    }
}
