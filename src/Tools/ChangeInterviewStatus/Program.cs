using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Flurl;
using Flurl.Http;
using Flurl.Http.Content;
using NConsole;
using Newtonsoft.Json;
using ZetaLongPaths;

namespace ChangeInterviewStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor(new ConsoleHost());
            processor.RegisterCommand<ApproveInterviewsCommand>("ApproveInterviews");
            processor.RegisterCommand<RejectInterviewsCommand>("RejectInterviews");
            processor.Process(args);
        }

        internal class ApproveInterviewsCommand : ChangeInterviewStatusCommand, IConsoleCommand
        {
            private static string ApproveUrl => "approve";

            public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
            {
                await Task.Run(() => base.Run(ApproveUrl));
            }
        }

        internal class RejectInterviewsCommand : ChangeInterviewStatusCommand, IConsoleCommand
        {
            private static string RejectUrl => "reject";
            public async Task RunAsync(CommandLineProcessor processor, IConsoleHost host)
            {
                await Task.Run(() => base.Run(RejectUrl));
            }
        }

        internal abstract class ChangeInterviewStatusCommand
        {
            [Description("HQ application host with http(s).Default value is https://ocalhost")]
            [Argument(Name = "host", DefaultValue = "https://localhost")]
            public string Host { get; set; }

            [Description("Login of API user. Default is apiuser")]
            [Argument(Name = "Login", DefaultValue = "apiuser")]
            public string Login { get; set; }

            [Description("Password of API user.")]
            [Argument(Name = "password")]
            public string Password { get; set; }

            [Description("Tab delimited file with interview ids and comments.")]
            [Argument(Name = "filePath")]
            public string FilePath { get; set; }

            [Description("Max number of parallel tasks.")]
            [Argument(Name = "file", DefaultValue = 20)]
            public int ParallelTasksLimit { get; set; }

            [Description("Max number of parallel tasks.")]
            [Argument(Name = "delimeter", DefaultValue = "\t")]
            public string Delimeter { get; set; }

            private ProcessStatus status = new ProcessStatus();
            private ConsoleRestServiceSettings restServiceSettings;

            private string EndpointUrl => $"{this.Host}/api/v1/interviews/";

            private CancellationTokenSource cancellationTokenSource;

            public void Run(string changeStatusUrl)
            {
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

                this.status.TotalInterviewsCount = interviewsToImport.Length;
                try
                {
                    int processedInterviewsCount = 0;
                    Stopwatch elapsedTime = Stopwatch.StartNew();

                    Parallel.ForEach(interviewsToImport,
                        new ParallelOptions { MaxDegreeOfParallelism = this.ParallelTasksLimit, CancellationToken = this.cancellationTokenSource.Token }, 
                        (importedInterview) =>
                        {
                            try
                            {
                                this.ChangeStatus(changeStatusUrl, importedInterview, credentials).RunSynchronously();
                            }
                            catch (Exception ex)
                            {
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
                            Console.SetCursorPosition(0, 0);
                            Console.Write(this.status.ToString());
                        });

                    Console.WriteLine();
                    var totalTime = new TimeSpan(elapsedTime.ElapsedTicks);
                    Console.WriteLine($"Total time: {totalTime.ToString("g")}");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
                finally
                {
                    this.status.IsInProgress = false;
                }
            }

            private async Task ChangeStatus(string url, InterviewInfo interviewInfo, RestCredentials credentials)
            {
                await this.ExecuteRequestAsync(
                    relativeUrl: url,
                    credentials: credentials,
                    method: HttpMethod.Post,
                    request: interviewInfo,
                    userCancellationToken: this.cancellationTokenSource.Token);
            }

            private async Task<HttpResponseMessage> ExecuteRequestAsync(
                string relativeUrl,
                HttpMethod method,
                object queryString = null,
                object request = null,
                RestCredentials credentials = null,
                CancellationToken? userCancellationToken = null)
            {
                var requestTimeoutToken = new CancellationTokenSource(this.restServiceSettings.Timeout).Token;
                var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(requestTimeoutToken, userCancellationToken ?? default(CancellationToken));

                var fullUrl = this.EndpointUrl
                    .AppendPathSegment(relativeUrl)
                    .SetQueryParams(queryString);

                var restClient = fullUrl
                    .WithTimeout(this.restServiceSettings.Timeout)
                    .WithHeader("Accept-Encoding", "gzip,deflate");

                if (credentials != null)
                    restClient.WithBasicAuth(credentials.Login, credentials.Password);

                HttpResponseMessage result = null;
                try
                {
                    result = await restClient.SendAsync(method, this.CreateJsonContent(request), linkedCancellationTokenSource.Token);
                }
                catch (OperationCanceledException ex)
                {
                    // throwed when receiving bytes in ReceiveBytesWithProgressAsync method and user canceling request
                    throw new RestException("Request canceled by user", type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                }
                catch (FlurlHttpException ex)
                {
                    if (ex.GetSelfOrInnerAs<TaskCanceledException>() != null)
                    {
                        if (requestTimeoutToken.IsCancellationRequested)
                        {
                            throw new RestException("Request timeout", type: RestExceptionType.RequestByTimeout,
                                statusCode: HttpStatusCode.RequestTimeout, innerException: ex);
                        }

                        if (userCancellationToken.HasValue && userCancellationToken.Value.IsCancellationRequested)
                        {
                            throw new RestException("Request canceled by user",
                                type: RestExceptionType.RequestCanceledByUser, innerException: ex);
                        }
                    }
                    else if (ex.Call.Response != null)
                    {
                        throw new RestException(ex.Call.ErrorResponseBody ?? ex.Call.Response.ReasonPhrase, statusCode: ex.Call.Response.StatusCode, innerException: ex);
                    }

                    throw new RestException(message: "Unexpected web exception", innerException: ex);
                }
                catch (Exception ex)
                {
                    throw new RestException(message: "Unexpected web exception", innerException: ex);
                }

                return result;
            }

            private HttpContent CreateJsonContent(object data)
            {
                var serializedData = JsonConvert.SerializeObject(data);
                return data == null ? null : new CapturedStringContent(serializedData, Encoding.UTF8, "application/json");
            }

            
            private InterviewInfo[] ParseFileWithInterviewsInfo(string filePath)
            {
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
                    Delimiter = this.Delimeter,
                    WillThrowOnMissingField = false
                };
            }
        }
    }

    public class RestCredentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }

    public class ProcessStatus
    {
        public ProcessStatus()
        {
            this.Errors = new List<InterviewStatusChangeError>();
        }

        public string InterviewImportProcessId { get; set; }
        public bool IsInProgress { get; set; } = false;
        public DateTime StartedDateTime { get; set; }
        public int TotalInterviewsCount { get; set; }
        public int ProcessedInterviewsCount { get; set; }
        public double TimePerInterview { get; set; }
        public double ElapsedTime { get; set; }
        public double EstimatedTime { get; set; }
        public List<InterviewStatusChangeError> Errors { get; set; }

        public override string ToString()
        {
            var finishTime = new TimeSpan((long)(this.EstimatedTime - this.ElapsedTime));
            return $"{this.ProcessedInterviewsCount}/{this.TotalInterviewsCount}. Errors: {this.Errors.Count}. End in: {finishTime.ToString("g")}";
        }
    }

    public class InterviewStatusChangeError
    {
        public string[] RawData { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class InterviewInfo
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
    }

    public static class ExceptionExtensions
    {
        public static TException GetSelfOrInnerAs<TException>(this Exception source)
            where TException : Exception
        {
            if (source is TException)
            {
                return (TException)source;
            }

            while (source.InnerException != null)
            {
                return source.InnerException.GetSelfOrInnerAs<TException>();
            }

            return null;
        }
    }

    public enum RestExceptionType
    {
        Unexpected = 0,
        RequestByTimeout,
        RequestCanceledByUser,
        NoNetwork,
        HostUnreachable,
        InvalidUrl
    }
    public class RestException : Exception
    {
        public RestException(string message, HttpStatusCode statusCode = HttpStatusCode.ServiceUnavailable,
            RestExceptionType type = RestExceptionType.Unexpected, Exception innerException = null)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
            this.Type = type;
        }

        public HttpStatusCode StatusCode { get; private set; }
        public RestExceptionType Type { get; private set; }
    }

    public class ConsoleRestServiceSettings
    {
        public TimeSpan Timeout => new TimeSpan(0, 0, 0, 30);

        public int BufferSize => 512;

        public bool AcceptUnsignedSslCertificate => false;
    }

}
