using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace WB.Services.Export.Host.Infra
{
    internal class WebConfigReader
    {
        public static void Read(IConfiguration configuration, string webConfigsPath)
        {
            foreach (var webConfigPath in webConfigsPath.Split(';'))
            {
                if (File.Exists(webConfigPath))
                {
                    try
                    {
                        var xml = XDocument.Load(webConfigPath);
                        Log.Logger.Information("Loading configuration values from " + webConfigPath);

                        FillConnectionString(configuration, xml);
                        FillAppSettings(configuration, xml);
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error(e, "Were not able to apply configurations");
                    }
                }
            }
        }

        private static readonly List<(string ownKey, string hqKey)> AppSettings = new List<(string ownKey, string hqKey)>
        {
            ("Storage:S3:Enabled", "Storage.S3.Enable"),
            ("Storage:S3:BucketName", "Storage.S3.BucketName"),
            ("Storage:S3:Prefix", "Storage.S3.Prefix"),
            ("Storage:S3:Folder", "Storage.S3.Folder"),
            ("AWS:AccessKey", "AWSAccessKey"),
            ("AWS:SecretKey", "AWSSecretKey")
        };

        private static void FillAppSettings(IConfiguration configuration, XDocument config)
        {
            try
            {
                var settings = config.Element("configuration")?.Element("appSettings");
                if (settings == null) return;

                var values = settings.Elements("add")
                    .ToDictionary(
                        x => x.Attribute("key")?.Value,
                        x => x.Attribute("value")?.Value);

                foreach (var appSetting in AppSettings)
                {
                    if (values.TryGetValue(appSetting.hqKey, out var value))
                    {
                        configuration[appSetting.ownKey] = value;
                        Log.Logger.Debug("Set {key} = {value}", appSetting.hqKey, value);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "There were an error while reading data from web.config");
            }
        }

        private static void FillConnectionString(IConfiguration configuration, XDocument config)
        {
            var connectionString = GetConnectionString(config);

            if (string.IsNullOrWhiteSpace(connectionString)) return;

            configuration.GetSection("ConnectionStrings")["DefaultConnection"] = connectionString;

            var connectionStringWithOutPassword = Regex.Replace(connectionString, "password=[^;]*", "Password=***", RegexOptions.IgnoreCase);
            Log.Logger.Debug("Using connections string: {connectionString}", connectionStringWithOutPassword);
        }

        private static string? GetConnectionString(XDocument config)
        {
            var connectionStrings = config.Element("configuration")?.Element("connectionStrings");
            var connectionString = connectionStrings?.Elements("add")
                ?.FirstOrDefault(e => e.Attribute("name")?.Value == "Postgres")
                ?.Attribute("connectionString")
                ?.Value;
            return connectionString;
        }

        public static string? ReadConnectionStringFromWebConfig(string webConfigsPath)
        {
            foreach (var webConfigPath in webConfigsPath.Split(';').Reverse())
            {
                if (File.Exists(webConfigPath))
                {
                    try
                    {
                        var xml = XDocument.Load(webConfigPath);
                        var connectionString = GetConnectionString(xml);
                        if (!string.IsNullOrWhiteSpace(connectionString))
                            return connectionString;
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Error(e, "Were not able to read connection string from web.config");
                        throw;
                    }
                }
            }

            return null;
        }
    }
}
