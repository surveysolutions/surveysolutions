using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using NConsole;
using ZetaLongPaths;

namespace ApiUtil.StatusChange
{
    internal abstract class ChangeInterviewStatusCommand
    {
        public class InterviewInfo
        {
            public Guid Id { get; set; }
            public string Comment { get; set; }
        }

        private const int MaxNumberOfParallelTasks = 20;
        private const int DefaultTriesNumberPerOperation = 1;
        private const int MaxTriesNumberPerOperation = 30;

        [Description("HQ application host with http(s)")]
        [Argument(Name = "host", DefaultValue = "http://localhost")]
        public string Host { get; set; }

        [Description("Login of API user. Default is apiuser")]
        [Argument(Name = "Login", DefaultValue = "apiuser")]
        public string Login { get; set; }

        [Description("Password of API user.")]
        [Argument(Name = "password")]
        public string Password { get; set; }

        [Description("Absolute path to tab delimited file with interview ids and comments.")]
        [Argument(Name = "filePath")]
        public string FilePath { get; set; }

        [Description("Absolute path to file to write all errors after command completion.")]
        [Argument(Name = "errorLog")]
        public string ErrorLogFilePath { get; set; }

        [Description("Max number of parallel tasks.")]
        [Argument(Name = "tasksLimit", DefaultValue = MaxNumberOfParallelTasks)]
        public int ParallelTasksLimit { get; set; }

        [Description("Max number of retries per operation (max 30)")]
        [Argument(Name = "retryLimit", DefaultValue = DefaultTriesNumberPerOperation)]
        public int OperationRetryLimit { get; set; }

        private static string Delimeter => "\t";
        private ProcessStatus status = new ProcessStatus();
        private ConsoleRestServiceSettings restServiceSettings;

        private string EndpointUrl => $"{this.Host}/api/v1/interviews/";

        private CancellationTokenSource cancellationTokenSource;

        public async Task RunAsync(string changeStatusUrl)
        {
            if (ZlpIOHelper.FileExists(this.ErrorLogFilePath))
            {
                ZlpIOHelper.DeleteFile(this.ErrorLogFilePath);
            }
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
            var interviewsToImport = this.ParseFileWithInterviewsInfo(this.FilePath);
            var retriesPerOperation = Math.Max(Math.Min(OperationRetryLimit, MaxTriesNumberPerOperation), 1);

            this.status.TotalInterviewsCount = interviewsToImport.Length;
            try
            {
                int processedInterviewsCount = 0;
                Stopwatch elapsedTime = Stopwatch.StartNew();

                var maxAmountOfParallelTasksFrom_1_To_20 = Math.Max(Math.Min(this.ParallelTasksLimit, MaxNumberOfParallelTasks), 1);
                await interviewsToImport.ForEachAsync(maxAmountOfParallelTasksFrom_1_To_20,
                    async (importedInterview) =>
                    {
                        bool hasError = false;
                        try
                        {
                            await this.ChangeStatus(changeStatusUrl, importedInterview, credentials, retriesPerOperation);
                        }
                        catch (Exception ex)
                        {
                            hasError = true;
                            var errorMessage =
                                $"Error during changing status of interview {importedInterview.Id}. " +
                                $"Exception: {ex.Message}";

                            this.status.Errors.Add(new InterviewStatusChangeError() { ErrorMessage = errorMessage });
                        }

                        Interlocked.Increment(ref processedInterviewsCount);
                        this.status.ProcessedInterviewsCount = processedInterviewsCount;
                        this.status.ElapsedTime = elapsedTime.ElapsedTicks;
                        this.status.TimePerInterview = this.status.ElapsedTime / this.status.ProcessedInterviewsCount;
                        this.status.EstimatedTime = this.status.TimePerInterview * this.status.TotalInterviewsCount;
                        Console.WriteLine(this.status.ToString());
                        Console.WriteLine("{0} - {1}. Took: {2:g} to execute", importedInterview.Id.ToString("N"), hasError ? "Error" : changeStatusUrl, new TimeSpan((long)this.status.TimePerInterview));
                    });

                Console.WriteLine();
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

        private async Task ChangeStatus(string url, InterviewInfo interviewInfo, RestCredentials credentials, int retriesPerOperation)
        {
            await Rest.ExecuteRequestAsync(
                endpointUrl: this.EndpointUrl, 
                relativeUrl: url, 
                method: HttpMethod.Post, 
                restServiceSettings: this.restServiceSettings,
                retriesPerOperation: retriesPerOperation, 
                credentials: credentials, 
                request: interviewInfo, 
                token: this.cancellationTokenSource.Token);
        }

        private InterviewInfo[] ParseFileWithInterviewsInfo(string filePath)
        {
            if (!(Delimeter == "\t" || Delimeter == ","))
                throw new ArgumentException($"Wrong delimiter '{Delimeter}'. Only ',' or tab symbol are allowed.");

            const string ID = "Id";
            const string COMMENT = "Comment";

            if (!ZlpIOHelper.FileExists(filePath))
            {
                throw new ArgumentException($"File {filePath} cannot be found");
            }
            var fileContent = ZlpIOHelper.ReadAllText(filePath);
            var rows = new List<InterviewInfo>();
            using (var csvReader = new CsvReader(new StringReader(fileContent), this.CreateCsvConfiguration()))
            {
                if (!csvReader.Read())
                {
                    throw new ArgumentException($"File {filePath} is empty");
                }

                var fieldHeaders = csvReader.FieldHeaders.Select(x => x.Trim()).ToArray();

                var amountOfHeaders = fieldHeaders.Length;

                if (amountOfHeaders > 2)
                {
                    throw new ArgumentException("Only 2 columns are expected");
                }

                if (!fieldHeaders.Contains(ID))
                {
                    throw new ArgumentException("Id column is missing");
                }

                if (fieldHeaders.Distinct().Count() != amountOfHeaders)
                {
                    throw new ArgumentException("Duplicating headers are not allowed");
                }

                var indexOfIdColumn = fieldHeaders.Select(x => x.ToLower()).ToList().IndexOf(ID.ToLower());
                var indexOfCommentColumn = fieldHeaders.Select(x => x.ToLower()).ToList().IndexOf(COMMENT.ToLower());

                if (indexOfIdColumn < 0)
                {
                    throw new ArgumentException("Id column is mandatory");
                }
                int rowCurrentRowNumber = 1;

                do
                {
                    var row = new InterviewInfo();
                    var record = csvReader.CurrentRecord;

                    for (int i = 0; i < amountOfHeaders; i++)
                    {
                        if (i == indexOfIdColumn)
                        {
                            Guid interviewId;
                            if (!Guid.TryParse(record[i], out interviewId))
                            {
                                throw new ArgumentException($"Id value '{record[i]}' cannot be parsed. Row number {rowCurrentRowNumber}");
                            }
                            row.Id = interviewId;
                        }
                        if (i == indexOfCommentColumn)
                        {
                            row.Comment = (record[i] ?? "").Trim();
                        }
                    }
                    rowCurrentRowNumber++;
                    rows.Add(row);
                } while (csvReader.Read());
            }
            return rows.ToArray();
        }

        private CsvConfiguration CreateCsvConfiguration()
        {
            return new CsvConfiguration
            {
                HasHeaderRecord = true,
                TrimFields = true,
                IgnoreQuotes = false,
                Delimiter = Delimeter,
                WillThrowOnMissingField = false
            };
        }
    }
}