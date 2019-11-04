using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using NConsole;
using NLog;

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

            var webConfig = XDocument.Load(Path.Combine(PathToHeadquarters, "Web.config"));

            var nlogConfigSection = webConfig.Descendants("nlog").FirstOrDefault();

            var hasNlogSettings = nlogConfigSection != null;

            string pathToNlogLogs = null;
            if (hasNlogSettings)
            {
                var lotDirectoryValue = nlogConfigSection.Descendants("variable").FirstOrDefault(x => x.Attribute("name")?.Value == "logDirectory")
                    ?.Attribute("value")
                    ?.Value;

                if (lotDirectoryValue != null)
                {
                    var noBaseDir = lotDirectoryValue.Replace("${basedir}", PathToHq)
                        .Replace("/", "\\");
                    pathToNlogLogs = Path.Combine(noBaseDir, "logs");
                }
            }
            
            //export service logs location
            string pathToExportLogs = Path.GetFullPath(Path.Combine(PathToHeadquarters, ".bin", "logs"));

            totalLogFilesCount = 0;
            
            if (pathToNlogLogs != null && Directory.Exists(pathToNlogLogs))
                totalLogFilesCount += Directory.EnumerateFiles(pathToNlogLogs).Count();

            if (Directory.Exists(pathToExportLogs))
                totalLogFilesCount += Directory.EnumerateFiles(pathToExportLogs).Count();

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
                await MoveLogFilesToTempDirAsync(pathToNlogLogs, tempLogsDirectory, "nlog");
                await MoveLogFilesToTempDirAsync(pathToExportLogs, tempLogsDirectory, "export");
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
                host.WriteLine();
                host.WriteLine($"Archived to {Path.Combine(tempSupportDirectory, archiveFileName)}");
            }
            catch (Exception e)
            {
                logger.Error(e, "Unexpected exception");
                host.WriteError("Unexpected exception. See error log for more details");
            }
            finally
            {
                DeleteTemporaryDirectoryWithLogFiles(tempLogsDirectory);
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
            Console.WriteLine("Copying logs from {0} to temp directory", logsDirectory);

            if (!Directory.Exists(logsDirectory)) return;
            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            foreach (var filename in Directory.EnumerateFiles(logsDirectory))
            {
                Console.WriteLine($"Copying log file to temporary directory: {Path.GetFileName(filename)}");

                var logsTempDirectory = Path.Combine(tempDirectory, logTypeName);
                if (!Directory.Exists(logsTempDirectory)) Directory.CreateDirectory(logsTempDirectory);

                try
                {
                    using (var sourceStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var logFile = Path.Combine(tempDirectory, logTypeName, filename.Replace(logsDirectory, "").TrimStart('\\'));

                        if (!Directory.Exists(Path.GetDirectoryName(logFile)))
                            Directory.CreateDirectory(Path.GetDirectoryName(logFile));

                        using (var destinationStream = File.Create(logFile))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                            Console.CursorLeft = 0;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to copy log file: {Path.GetFileName(filename)}");
                    Console.WriteLine(e.Message);
                    logger.Error(e);
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
