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
    public class ArchiveLogsCommand : IConsoleCommand
    {
        private int _consoleCursorPosition;
        private int _logFilesCount;
        private int _totalLogFilesCount;

        private readonly IConfigurationManagerSettings _configurationManagerSettings;
        private readonly ILogger _logger;

        public ArchiveLogsCommand(IConfigurationManagerSettings configurationManagerSettings, ILogger logger)
        {
            _configurationManagerSettings = configurationManagerSettings;
            _logger = logger;
        }

        [Description("Physical path to Headquarters website.")]
        [Argument(Name = "path")]
        public string PathToHeadquarters { get; set; }

        public async Task<object> RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            this.PathToHeadquarters = this.PathToHeadquarters.TrimEnd('\\');

            _configurationManagerSettings.SetPhysicalPathToWebsite(this.PathToHeadquarters);

            var elmahConfigSection = ConfigurationManager.GetSection("elmah/errorLog") as Hashtable;
            var nlogConfigSection = ConfigurationManager.GetSection("nlog") as XmlLoggingConfiguration;

            var elmahRelativeLogPath = "logPath";
            var nlogLogPathFormat = "logDirectory";

            var hasElmahSettings = elmahConfigSection != null && elmahConfigSection.ContainsKey(elmahRelativeLogPath);
            var hasNlogSettings = nlogConfigSection != null &&
                                  (nlogConfigSection.Variables?.ContainsKey(nlogLogPathFormat) ?? false);

            if (!hasElmahSettings || !hasNlogSettings)
                host.WriteLine("Headquarters website settings not found. " +
                               "Please, ensure that you enter correct path to Headquarters website");
            else
            {
                var pathToElmahLogs = ((string) elmahConfigSection[elmahRelativeLogPath])
                    .Replace("~", this.PathToHeadquarters).Replace("/", "\\");

                var pathToNlogLogs = nlogConfigSection.Variables[nlogLogPathFormat]
                                         .Text.Replace("${basedir}", this.PathToHeadquarters)
                                         .Replace("/", "\\")
                                         .TrimEnd('\\') + "\\logs";

                var tempSupportDirectory = Path.Combine(Path.GetTempPath(), "Survey Solutions Support");
                var tempLogsDirectory = Path.Combine(tempSupportDirectory, "logs");
                var archiveFileName = $"{DateTime.Now:yyyy-MM-ddThhmmss}-headquarters-logs.zip";

                _totalLogFilesCount = Directory.EnumerateFiles(pathToElmahLogs).Count() +
                                      Directory.EnumerateFiles(pathToNlogLogs).Count();

                try
                {
                    await MoveLogFilesToTempDirAsync(pathToElmahLogs, tempLogsDirectory, "elmah");
                    await MoveLogFilesToTempDirAsync(pathToNlogLogs, tempLogsDirectory, "nlog");
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unexpected exception");
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
                    _logger.Error(e, "Unexpected exception");
                    host.WriteError("Unexpected exception. See error log for more details");
                }

                host.WriteLine();
            }

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
                        Console.Write($"Moving log files to temporary directory: {++_logFilesCount} of {_totalLogFilesCount}");
                    }
                }
            }
        }

        public void ArchiveWithProgress(string backupFolderPath, string backupFileName)
        {
            _consoleCursorPosition = Console.CursorLeft;
            _logFilesCount = 0;
            _totalLogFilesCount = Directory.GetFiles(backupFolderPath, "*.*", SearchOption.AllDirectories).Length;

            FastZipEvents events = new FastZipEvents {ProcessFile = ProcessFileMethod};
            FastZip fastZip = new FastZip(events) {CreateEmptyDirectories = true};

            string zipFileName = Path.Combine(Directory.GetParent(backupFolderPath).FullName, backupFileName);

            fastZip.CreateZip(zipFileName, backupFolderPath, true, "");
        }
        
        private void ProcessFileMethod(object sender, ScanEventArgs args)
        {
            Console.CursorLeft = _consoleCursorPosition;
            Console.Write($"{++_logFilesCount * 100 / _totalLogFilesCount}%");
        }
    }
}