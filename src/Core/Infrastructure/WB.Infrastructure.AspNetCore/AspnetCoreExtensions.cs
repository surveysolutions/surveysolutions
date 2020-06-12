using System.Diagnostics;
using System.IO;
using System.Reflection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;

namespace WB.Infrastructure.AspNetCore
{
    public static class AspnetCoreExtensions
    {
        public static LoggerConfiguration ConfigureSurveySolutionsLogging(this LoggerConfiguration logger, string appRoot, string projectName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var verboseLog = Path.Combine(appRoot, "..", "logs", $"{projectName.ToLower()}.verbose.json..log");
            var logsFileLocation = Path.Combine(appRoot, "..", "logs", $"{projectName.ToLower()}..log");

            return logger
                .Destructure.ToMaximumDepth(4)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("Version", fvi.FileVersion)
                .Enrich.WithProperty("VersionInfo", fvi.ProductVersion)
                .Enrich.WithProperty("AppType", projectName)
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Quartz.Core", LogEventLevel.Warning)
                .MinimumLevel.Override("Anemonis.AspNetCore", LogEventLevel.Warning)
                .WriteTo.File(logsFileLocation, rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo
                    .File(new RenderedCompactJsonFormatter(), Path.GetFullPath(verboseLog), LogEventLevel.Verbose,
                        retainedFileCountLimit: 3, rollingInterval: RollingInterval.Day)
                ;    
        }
    }
}
