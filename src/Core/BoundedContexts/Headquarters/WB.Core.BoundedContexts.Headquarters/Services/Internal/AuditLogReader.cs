using System;
using System.IO;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace WB.Core.BoundedContexts.Headquarters.Services.Internal
{
    public class AuditLogReader : IAuditLogReader
    {
        public string[] Read()
        {
            var filePath = GetServerFilePath();

            if (File.Exists(filePath))
            {
                var result = File.ReadAllLines(filePath);
                return result;
            }
            return Array.Empty<string>();
        }

        public string GetServerFilePath()
        {
            var initialTarget = (AsyncTargetWrapper) LogManager.Configuration.FindTargetByName("auditTarget");
            var fileTarget = (FileTarget) initialTarget.WrappedTarget;
            var filePath = fileTarget.FileName.Render(new LogEventInfo());
            return Path.GetFullPath(filePath);
        }
    }
}