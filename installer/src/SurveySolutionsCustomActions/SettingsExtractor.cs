using System;
using System.IO;
using System.Xml.XPath;
using Microsoft.Deployment.WindowsInstaller;

namespace SurveySolutionsCustomActions
{
    public class SettingsExtractor : ISettingsExtractor
    {
        public PreviousConfiguration GetPreviousConfiguration(Session session)
        {
            // Try new INI format first (current format used since ~21.x)
            var iniFilePath = session.CustomActionData["SourceIniFile"];
            if (!string.IsNullOrEmpty(iniFilePath) && File.Exists(iniFilePath))
            {
                session.Log($"Reading connection string from INI config file {iniFilePath}");
                return GetIniConfig(iniFilePath);
            }

            // Fall back to old XML format for upgrades from very old versions
            var xmlFilePath = session.CustomActionData["SourceFile"];
            if (!File.Exists(xmlFilePath))
            {
                session.Log($"Config file was not found {xmlFilePath}");
                return null;
            }

            session.Log($"Reading connection string from XML config file {xmlFilePath}");
            return GetConfig(xmlFilePath);
        }

        public PreviousConfiguration GetConfig(string filePath)
        {
            var navigator = new XPathDocument(filePath).CreateNavigator();

            var config = new PreviousConfiguration()
            {
                ConnectionString = navigator.SelectSingleNode("/configuration/connectionStrings/add/@connectionString")?.Value,
                BaseURL = navigator.SelectSingleNode("/configuration/appSettings/add[@key='BaseUrl']/@value")?.Value,
                TenantName = navigator.SelectSingleNode("/configuration/appSettings/add[@key='Storage.S3.Prefix']/@value")?.Value,
                DesignerAddress = navigator.SelectSingleNode("/configuration/appSettings/add[@key='DesignerAddress']/@value")?.Value
            };

            return config;
        }

        public PreviousConfiguration GetIniConfig(string filePath)
        {
            var config = new PreviousConfiguration();
            string currentSection = null;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var trimmedLine = line.Trim();

                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    continue;
                }

                var equalsIndex = trimmedLine.IndexOf('=');
                // Skip lines with no '=' sign, or lines where the key is empty (equalsIndex == 0)
                if (equalsIndex <= 0) continue;

                var key = trimmedLine.Substring(0, equalsIndex).Trim();
                var value = trimmedLine.Substring(equalsIndex + 1).Trim();

                switch (currentSection)
                {
                    case "Headquarters":
                        if (key.Equals("BaseUrl", StringComparison.OrdinalIgnoreCase)) config.BaseURL = value;
                        if (key.Equals("TenantName", StringComparison.OrdinalIgnoreCase)) config.TenantName = value;
                        break;
                    case "Designer":
                        if (key.Equals("DesignerAddress", StringComparison.OrdinalIgnoreCase)) config.DesignerAddress = value;
                        break;
                    case "ConnectionStrings":
                        if (key.Equals("DefaultConnection", StringComparison.OrdinalIgnoreCase)) config.ConnectionString = value;
                        break;
                }
            }

            return config;
        }
    }

    public interface ISettingsExtractor
    {
        PreviousConfiguration GetPreviousConfiguration(Session session);
        PreviousConfiguration GetConfig(string filePath);
        PreviousConfiguration GetIniConfig(string filePath);
    }
}
