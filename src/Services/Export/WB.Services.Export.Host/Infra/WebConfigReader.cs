using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WB.Services.Export.Host.Infra
{
    internal class WebConfigReader
    {
        public static void Read(IConfiguration configuration, string webConfigsPath, ILogger logger)
        {
            foreach (var webConfigPath in webConfigsPath.Split(';'))
            {
                if (File.Exists(webConfigPath))
                {
                    try
                    {
                        var xml = XDocument.Load(webConfigPath);
                        logger.LogInformation("Loading configuration values from " + webConfigPath);

                        FillConnectionString(configuration, xml, logger);
                        FillAppSettings(configuration, xml, logger);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Were not able to apply configurations");
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

        private static void FillAppSettings(IConfiguration configuration, XDocument config, ILogger logger)
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
                        logger.LogDebug("Set {key} = {value}", appSetting.hqKey, value);
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "There were an error while reading data from web.config");
            }
        }

        private static void FillConnectionString(IConfiguration configuration, XDocument config, ILogger logger)
        {  
            var connectionStrings = config.Element("configuration")?.Element("connectionStrings");
            if (connectionStrings == null) return;

            var connectionString = connectionStrings
                .Elements("add")
                ?.FirstOrDefault(e => e.Attribute("name")?.Value == "Postgres")
                ?.Attribute("connectionString")
                ?.Value;

            if (string.IsNullOrWhiteSpace(connectionString)) return;

            configuration.GetSection("ConnectionStrings")["DefaultConnection"] = connectionString;
            logger.LogDebug("Using connections string: {connectionString}", connectionString);
        }
    }
}
