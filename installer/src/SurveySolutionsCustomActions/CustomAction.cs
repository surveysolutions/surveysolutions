using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Resolvers;
using Microsoft.Deployment.WindowsInstaller;

namespace SurveySolutionsCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult WriteSiteIniSettings(Session session)
        {
            session.Log("Begin WriteIniSettings action");

            try
            {
                var filePath = ValidateTargetFileAndGetFilePath(session, "TargetFile");
                if (string.IsNullOrEmpty(filePath))
                    return ActionResult.SkipRemainingActions;

                var config = new SettingsExtractor().GetPreviousConfiguration(session);

                var sitePort = session.CustomActionData["SitePort"];
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Headquarters]");
                var baseUrl = config?.BaseURL ?? $"http://localhost:{sitePort}" ;
                sb.AppendLine($"BaseUrl={baseUrl}");
                var tenantName = config?.TenantName ?? "hq";
                sb.AppendLine($"TenantName={tenantName}");
                sb.AppendLine("[Apks]");
                sb.AppendLine("ClientApkPath=Client");
                sb.AppendLine("[Designer]");
                var designerAddress = config?.DesignerAddress ?? "https://designer.mysurvey.solutions";
                sb.AppendLine($"DesignerAddress={designerAddress}");
                sb.AppendLine("[FileStorage]");
                sb.AppendLine("AppData=..\\Data_Site");
                sb.AppendLine("TempData=..\\Data_Site");
                sb.AppendLine("[ConnectionStrings]");

                var connectionStr = config?.ConnectionString ?? 
                                    session.CustomActionData["CONNECTIONSTRING"];
                sb.AppendLine($"DefaultConnection={connectionStr}");

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception e)
            {
                session.Log("Error on Action Execution.", e.Message);
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UpdateWebConfig(Session session)
        {
            var installFolder = session.CustomActionData["InstallFolder"];

            var webConfig = Path.Combine(installFolder, "web.config");

            if (File.Exists(webConfig))
            {
                UpdateWebConfigContent(webConfig);
            }

            return ActionResult.Success;
        }
      
        private static string ValidateTargetFileAndGetFilePath(Session session, string data )
        {
            var filePath = session.CustomActionData[data];

            //FileInfo(filePath).Length returns not 0 for compressed FS and NTFS
            if (File.Exists(filePath) && File.ReadAllText(filePath).Length > 0)
            {
                return null;
            }

            return filePath;
        }

        public static void UpdateWebConfigContent(string webConfigPath)
        {
            var info = new FileInfo(webConfigPath);

            var xml = XDocument.Load(info.FullName);

            var aspNetCore = xml.Descendants("aspNetCore").First();

            var envVariables = aspNetCore.Element("environmentVariables");

            if (envVariables == null)
            {
                envVariables = new XElement("environmentVariables");
                aspNetCore.Add(envVariables);
            }

            var envVar = envVariables.Elements("environmentVariable")
                .SingleOrDefault(x => x.Attribute("name")?.Value == "DOTNET_BUNDLE_EXTRACT_BASE_DIR");

            if (envVar == null)
            {
                envVar = new XElement("environmentVariable",
                    new XAttribute("name", "DOTNET_BUNDLE_EXTRACT_BASE_DIR"),
                    new XAttribute("value", ""));

                envVariables.Add(envVar);
            }

            envVar.SetAttributeValue("value", Path.Combine(info.Directory.FullName, ".net-app"));

            xml.Save(info.FullName);
        }
    }
}
