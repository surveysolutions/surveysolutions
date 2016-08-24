using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiUtil.StatusChange;
using NConsole;
using ZetaLongPaths;

namespace ApiUtil.Export
{
    public class ExportDetails
    {
        public bool HasExportedFile { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public ExportStatus ExportStatus { get; set; }
        public RunningProcess RunningProcess { get; set; }
    }

    public enum ExportStatus
    {
        NotStarted = 1,
        Queued = 2,
        Running = 3,
        Finished = 4,
        FinishedWithErrors = 5
    }

    public enum ExportType
    {
        tabular = 1,
        spss = 2,
        stata = 3,
        binary = 4,
        paradata = 5
    }

    public class RunningProcess
    {
        public DateTime StartDate { get; set; }
        public int ProgressInPercents { get; set; }
    }

    internal class ExportDataCommand : IConsoleCommand
    {
        [Description("HQ application host with http(s)")]
        [Argument(Name = "host", DefaultValue = "http://localhost")]
        public string Host { get; set; }

        [Description("Login of API user. Default is apiuser")]
        [Argument(Name = "Login", DefaultValue = "apiuser")]
        public string Login { get; set; }

        [Description("Password of API user.")]
        [Argument(Name = "password")]
        public string Password { get; set; }

        [Description("Questionnaire identity. Combination of questionnaire id and version separated with '$' sign. Example: aaaaaaaaaaaaaaaaaaaaaaaaaaaaa$2")]
        [Argument(Name = "id")]
        public string QuestionnaireId { get; set; }

        [Description("Export type: tabular, spss, stata, binary, paradata")]
        [Argument(Name = "type")]
        public ExportType ExportType { get; set; }

        [Description("If export file were created within specified interval, new process will not be started and existing file will be downloaded. Time in minutes")]
        [Argument(Name = "relevanceTime", DefaultValue = 30)]
        public int ExportFileRelevanceTime { get; set; }

        [Description("Absolute path to file to write all errors after command completion.")]
        [Argument(Name = "errorLog")]
        public string ErrorLogFilePath { get; set; }

        [Description("Delay time between requests. Can't be less than 1000 (1 sec) and more than 60000 (60 sec)")]
        [Argument(Name = "delay", DefaultValue = 1000)]
        public int Delay { get; set; }

        [Description("Absotute path where export file should be saved. Example: c:\\Temp\\export.zip")]
        [Argument(Name = "pathToExportFile")]
        public string AbsolutePathToExportFile { get; set; }

    private ProcessStatus status = new ProcessStatus();
        private ConsoleRestServiceSettings restServiceSettings;
        private string EndpointUrl => $"{this.Host}/api/v1/export/";
        private CancellationTokenSource cancellationTokenSource;

        private string DetailsUrl(string exportType, string questionnaireId) => $"{exportType}/{questionnaireId}/details";
        private string StartUrl(string exportType, string questionnaireId) => $"{exportType}/{questionnaireId}/start";
        private string DownloadUrl(string exportType, string questionnaireId) => $"{exportType}/{questionnaireId}";
        
        public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
        {
            Delay = Math.Max(1000, Math.Min(Delay, 60000));
            this.restServiceSettings = new ConsoleRestServiceSettings();
            this.cancellationTokenSource = new CancellationTokenSource();

            var credentials = new RestCredentials
            {
                Login = this.Login,
                Password = this.Password
            };

            this.status = new ProcessStatus
            {
                StartedDateTime = DateTime.Now,
                ProcessedInterviewsCount = 0,
                ElapsedTime = 0,
                EstimatedTime = 0,
                Errors = new List<InterviewStatusChangeError>(),
                IsInProgress = true
            };

            Console.Clear();

            try
            {
                Stopwatch elapsedTime = Stopwatch.StartNew();

                var exportDetails = await Rest.GetAsync<ExportDetails>(
                    endpointUrl: this.EndpointUrl,
                    relativeUrl: DetailsUrl(ExportType.ToString(), QuestionnaireId),
                    restServiceSettings: this.restServiceSettings,
                    retriesPerOperation: 3,
                    credentials: credentials,
                    token: this.cancellationTokenSource.Token);

                bool shouldSkipStart = false;
                bool shouldSkipMonitor = false;
                if (exportDetails.HasExportedFile)
                {
                    var minimunRelevantDate = DateTime.Now.AddMinutes(-1 * ExportFileRelevanceTime);
                    var isExportedFilesIsRelevant = minimunRelevantDate < exportDetails.LastUpdateDate;
                    if (isExportedFilesIsRelevant)
                    {
                        shouldSkipStart = true;
                        shouldSkipMonitor = true;
                    }
                }
                else
                    switch (exportDetails.ExportStatus)
                    {
                        case ExportStatus.NotStarted:
                        case ExportStatus.Queued:
                        case ExportStatus.Running:
                            break;
                        case ExportStatus.Finished:
                        case ExportStatus.FinishedWithErrors:
                            shouldSkipStart = true;
                            break;
                    }

                if (!shouldSkipStart)
                {
                    await StartNewExportAsync(credentials);
                }
                if (!shouldSkipMonitor)
                {
                    await MonitorExportAsync(credentials);
                }

                await DownloadExportFileAsync(credentials);

                var timePerInterview = new TimeSpan((long)this.status.TimePerInterview);
                Console.WriteLine($"Average time per interview: {timePerInterview:g}");
                var totalTime = new TimeSpan(elapsedTime.ElapsedTicks);
                Console.WriteLine($"Total time: {totalTime:g}");
            }
            catch (Exception exception)
            {
                Console.WriteLine("Process was aborted.");
                Console.WriteLine(exception.Message);
            }
            finally
            {
                this.status.IsInProgress = false;
                if (this.status.Errors.Any())
                {
                    ZlpIOHelper.WriteAllText(this.ErrorLogFilePath, string.Join(Environment.NewLine, this.status.Errors.Select(x => x.ErrorMessage)));
                }
            }
            Console.Write("Completed");
            if (status.Errors.Any())
            {
                Console.WriteLine($" with errors. Errors log can be found at '{ErrorLogFilePath}'");
            }
        }

        private async Task MonitorExportAsync(RestCredentials credentials)
        {
            var status = ExportStatus.NotStarted;
            while (status != ExportStatus.Finished && status!= ExportStatus.FinishedWithErrors)
            {
                var exportDetails = await Rest.GetAsync<ExportDetails>(
                   endpointUrl: this.EndpointUrl,
                   relativeUrl: DetailsUrl(ExportType.ToString(), QuestionnaireId),
                   restServiceSettings: this.restServiceSettings,
                   retriesPerOperation: 3,
                   credentials: credentials,
                   token: this.cancellationTokenSource.Token);

                status = exportDetails.ExportStatus;
                Console.Write($"{QuestionnaireId}[{ExportType}]: {status}");
                if (exportDetails.ExportStatus == ExportStatus.Running && exportDetails.RunningProcess!=null)
                {
                    Console.WriteLine($" {exportDetails.RunningProcess.ProgressInPercents}%");
                }
                else
                {
                    Console.WriteLine();
                }

                await Task.Delay(Delay);
            }
        }

        private async Task StartNewExportAsync(RestCredentials credentials)
        {
            var startExportInfo = await Rest.ExecuteRequestAsync(endpointUrl: this.EndpointUrl, relativeUrl: StartUrl(ExportType.ToString(), QuestionnaireId), restServiceSettings: this.restServiceSettings, retriesPerOperation: 3, credentials: credentials, method: HttpMethod.Post, token: this.cancellationTokenSource.Token);

            if (startExportInfo.IsSuccessStatusCode)
            {
                await MonitorExportAsync(credentials);
            }
        }

        private async Task DownloadExportFileAsync(RestCredentials credentials)
        {
            RestFile file = await Rest.DownloadFileAsync(endpointUrl: this.EndpointUrl, relativeUrl: DownloadUrl(ExportType.ToString(), QuestionnaireId),
                    restServiceSettings: this.restServiceSettings, retriesPerOperation: 3, credentials: credentials,
                    token: this.cancellationTokenSource.Token);

            File.WriteAllBytes(AbsolutePathToExportFile, file.Content);
        }
    }
}