using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NConsole;
using NLog;
using NLog.Config;

namespace support
{
    [Description("Archive Headquarters log files.")]
    public class ArchiveLogsCommand : ConfigurationDependentCommand, IConsoleCommand
    {
        private int consoleCursorPosition;
        private int logFilesCount;
        private int totalLogFilesCount;

        private readonly ILogger logger;

        public ArchiveLogsCommand(IConfigurationManagerSettings configurationManagerSettings, ILogger logger) : base(configurationManagerSettings)
        {
            this.logger = logger;
        }

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            if (!ReadConfigurationFile(host))
                return null;

            var elmahConfigSection = ConfigurationManager.GetSection("elmah/errorLog") as Hashtable;
            var nlogConfigSection = ConfigurationManager.GetSection("nlog") as XmlLoggingConfiguration;

            var elmahRelativeLogPath = "logPath";
            var nlogLogPathFormat = "logDirectory";

            var hasElmahSettings = elmahConfigSection != null && elmahConfigSection.ContainsKey(elmahRelativeLogPath);
            var hasNlogSettings = nlogConfigSection != null &&
                                  (nlogConfigSection.Variables?.ContainsKey(nlogLogPathFormat) ?? false);

            string pathToElmahLogs = "";

            if (hasElmahSettings)
                pathToElmahLogs = ((string) elmahConfigSection[elmahRelativeLogPath]).Replace("~", PathToHq).Replace("/", "\\");

            string pathToNlogLogs = "";
            if (hasNlogSettings)
                pathToNlogLogs = nlogConfigSection.Variables[nlogLogPathFormat]
                                     .Text.Replace("${basedir}", PathToHq)
                                     .Replace("/", "\\")
                                     .TrimEnd('\\') + "\\logs";

            totalLogFilesCount = 0;
            if (Directory.Exists(pathToElmahLogs))
                totalLogFilesCount += Directory.EnumerateFiles(pathToElmahLogs).Count();

            if (Directory.Exists(pathToNlogLogs))
                totalLogFilesCount += Directory.EnumerateFiles(pathToNlogLogs).Count();

            if (totalLogFilesCount == 0)
            {
                host.WriteLine("No logs found");
                return null;
            }

            var tempSupportDirectory = Path.Combine(Path.GetTempPath(), "Survey Solutions Support");
            var tempLogsDirectory = Path.Combine(tempSupportDirectory, "logs");
            var archiveFileName = $"{DateTime.Now:yyyy-MM-ddThhmmss}-headquarters-logs.zip";

            try
            {
                await MoveLogFilesToTempDirAsync(pathToElmahLogs, tempLogsDirectory, "elmah");
                await MoveLogFilesToTempDirAsync(pathToNlogLogs, tempLogsDirectory, "nlog");
            }
            catch (Exception e)
            {
                logger.Error(e, "Unexpected exception");
                host.WriteError("Unexpected exception. See error log for more details");
            }

            host.WriteLine();

            try
            {
                host.WriteMessage("Archiving files: ");
                ArchiveWithProgress(tempLogsDirectory, archiveFileName);
                DeleteTemporaryDirectoryWithLogFiles(tempLogsDirectory);
                host.WriteLine();
                host.WriteLine($"Archived to {Path.Combine(tempSupportDirectory, archiveFileName)}");
            }
            catch (Exception e)
            {
                logger.Error(e, "Unexpected exception");
                host.WriteError("Unexpected exception. See error log for more details");
            }

            host.WriteLine();
            
            return null;
        }

        private void DeleteTemporaryDirectoryWithLogFiles(string tempLogsDirectory)
        {
            DirectoryInfo di = new DirectoryInfo(tempLogsDirectory);
            di.GetFiles().ToList().ForEach(f => f.Delete());
            di.GetDirectories().ToList().ForEach(d => d.Delete(true));
            di.Delete(true);
        }

        private async Task MoveLogFilesToTempDirAsync(string logsDirectory, string tempDirectory, string logTypeName)
        {
            if (!Directory.Exists(logsDirectory)) return;
            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            foreach (var filename in Directory.EnumerateFiles(logsDirectory))
            {
                var logsTempDirectory = Path.Combine(tempDirectory, logTypeName);
                if (!Directory.Exists(logsTempDirectory)) Directory.CreateDirectory(logsTempDirectory);

                using (var sourceStream = File.Open(filename, FileMode.Open))
                {
                    var logFile = Path.Combine(tempDirectory, logTypeName, filename.Replace(logsDirectory, "").TrimStart('\\'));

                    if (!Directory.Exists(Path.GetDirectoryName(logFile)))
                        Directory.CreateDirectory(Path.GetDirectoryName(logFile));

                    using (var destinationStream = File.Create(logFile))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                        Console.CursorLeft = 0;
                        Console.Write($"Moving log files to temporary directory: {++logFilesCount} of {totalLogFilesCount}");
                    }
                }
            }
        }

        public void ArchiveWithProgress(string backupFolderPath, string backupFileName)
        {
            consoleCursorPosition = Console.CursorLeft;
            logFilesCount = 0;
            totalLogFilesCount = Directory.GetFiles(backupFolderPath, "*.*", SearchOption.AllDirectories).Length;

            FastZipEvents events = new FastZipEvents {ProcessFile = ProcessFileMethod};
            FastZip fastZip = new FastZip(events) {CreateEmptyDirectories = true};

            string zipFileName = Path.Combine(Directory.GetParent(backupFolderPath).FullName, backupFileName);

            fastZip.CreateZip(zipFileName, backupFolderPath, true, "");
        }
        
        private void ProcessFileMethod(object sender, ScanEventArgs args)
        {
            Console.CursorLeft = consoleCursorPosition;
            Console.Write($"{++logFilesCount * 100 / totalLogFilesCount}%");
        }
    }
}
