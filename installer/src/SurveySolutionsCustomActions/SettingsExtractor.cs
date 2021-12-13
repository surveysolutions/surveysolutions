using System.IO;
using System.Xml.XPath;
using Microsoft.Deployment.WindowsInstaller;

namespace SurveySolutionsCustomActions
{
    public class SettingsExtractor : ISettingsExtractor
    {
        public PreviousConfiguration GetPreviousConfiguration(Session session)
        {
            var filePath = session.CustomActionData["SourceFile"];

            if (!File.Exists(filePath))
            {
                session.Log($"Config file was not found {filePath}");
                return null;
            }

            session.Log($"Reading connection string from config file {filePath}");

            return GetConfig(filePath);
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
    }

    public interface ISettingsExtractor
    {
        PreviousConfiguration GetPreviousConfiguration(Session session);
        PreviousConfiguration GetConfig(string filePath);
    }
}
