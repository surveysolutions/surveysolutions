using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.XPath;
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

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("[Headquarters]");
                sb.AppendLine("BaseUrl=http://localhost:9700");
                sb.AppendLine("TenantName=hq");
                sb.AppendLine("[DataExport]");
                sb.AppendLine("ExportServiceUrl=http://localhost:5000");
                sb.AppendLine("[ConnectionString]");

                var connectionStr = GetConnectionString(session);
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
        public static ActionResult WriteExportIniSettings(Session session)
        {
            session.Log("Begin WriteExportIniSettings  action");
            Debugger.Launch()
            try
            {
                var filePath = ValidateTargetFileAndGetFilePath(session);
                if (string.IsNullOrEmpty(filePath))
                    return ActionResult.SkipRemainingActions;

                StringBuilder sb = new StringBuilder();

                sb.AppendLine("Port=5000");
                sb.AppendLine();
                sb.AppendLine("[ConnectionStrings]");

                var connectionStr = GetConnectionString(session);
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

        private static string GetConnectionString(Session session)
        {
            var filePath = session.CustomActionData["SourceFile"];

            if (!File.Exists(filePath)) 
                return session.CustomActionData["CONNECTIONSTRING"];
            session.Log($"Reading connection string from config file {filePath}");
            
            var node = new XPathDocument(filePath).CreateNavigator().SelectSingleNode("/configuration/connectionStrings/add/@connectionString");
            
            if (node == null) 
                return session.CustomActionData["CONNECTIONSTRING"];
                
            session.Log("Connection string found.");
            return node.Value;
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
