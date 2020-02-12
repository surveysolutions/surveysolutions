using System.IO;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace SurveySolutionsCustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult WriteIniSettings(Session session)
        {
            session.Log("Begin WriteIniSettings action");

            //System.Diagnostics.Debugger.Launch();

            var filePath= session.CustomActionData["TargetFile"];

            if (File.Exists(filePath))
                return ActionResult.SkipRemainingActions;
            
            string connectionString = session.CustomActionData["CONNECTIONSTRING"];
            string oldConnectionString = session.CustomActionData["OLDCONFIGCONNECTIONSTRING"];

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Headquarters]");
            sb.AppendLine("BaseUrl=http://localhost:9700");
            sb.AppendLine("TenantName=hq");
            sb.AppendLine("[DataExport]");
            sb.AppendLine("ExportServiceUrl=http://localhost:5000");
            sb.AppendLine("[ConnectionString]");

            var connectionStr = string.IsNullOrEmpty(oldConnectionString) ? connectionString : oldConnectionString;
            sb.AppendLine($"DefaultConnection={connectionStr}");

            File.WriteAllText(filePath, sb.ToString());

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult WriteJsonSettings(Session session)
        {
            session.Log("Begin WriteJsonSettings  action");

            //System.Diagnostics.Debugger.Launch();

            var filePath = session.CustomActionData["TargetFile"];

            if (File.Exists(filePath))
                return ActionResult.SkipRemainingActions;

            string connectionString = session.CustomActionData["CONNECTIONSTRING"];
            string oldConnectionString = session.CustomActionData["OLDCONFIGCONNECTIONSTRING"];

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("\"Port\": 5000,");
            sb.AppendLine("\"ConnectionStrings\": {");

            var connectionStr = string.IsNullOrEmpty(oldConnectionString) ? connectionString : oldConnectionString;
            sb.AppendLine($"\"DefaultConnection\":\"{connectionStr}\"");
            sb.AppendLine("}");
            sb.AppendLine("}");

            File.WriteAllText(filePath, sb.ToString());

            return ActionResult.Success;
        }
    }
}
