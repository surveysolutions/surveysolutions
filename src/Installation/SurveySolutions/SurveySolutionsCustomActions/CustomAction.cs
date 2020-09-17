using System;
using System.IO;
using System.Text;
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
                var filePath = ValidateTargetFileAndGetFilePath(session);
                if (string.IsNullOrEmpty(filePath))
                    return ActionResult.SkipRemainingActions;

                var config = new SettingsExtractor().GetPreviousConfiguration(session);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Headquarters]");
                var baseUrl = config?.BaseURL ?? "http://localhost:9700";
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

      
        private static string ValidateTargetFileAndGetFilePath(Session session)
        {
            var filePath = session.CustomActionData["TargetFile"];

            //FileInfo(filePath).Length returns not 0 for compressed FS and NTFS
            if (File.Exists(filePath) && File.ReadAllText(filePath).Length > 0)
            {
                return null;
            }

            return filePath;
        }
    }
}
