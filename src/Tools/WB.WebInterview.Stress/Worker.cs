using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

using Serilog;

namespace WB.WebInterview.Stress
{
    [Localizable(false)]
    public class Worker
    {
        private static long _workerCounter;

        private static readonly Random Rnd = new Random();
        private readonly Configuration _config;
        private readonly ApiMaster _apiMaster;
        private readonly HashSet<string> _sharedInterviews;
        private readonly ILogger logger;
        private readonly CancellationToken _cancellationToken;
        
        public string WorkerId { get; }

        private readonly Stopwatch _restartWatcher = new Stopwatch();
        private readonly Stopwatch _sw = new Stopwatch();
        private HubConnection _client;
        public string InterviewId { get; private set; }
        private IHubProxy _proxy;
        private int _liveTime;
        private string _interviewLocation;
        private int? _inDelay;

        private static readonly Stopwatch _global = new Stopwatch();

        public Worker(Configuration configuration, ApiMaster apiMaster, HashSet<string> sharedInterviews, ILogger logger, CancellationToken cancellationToken = default(CancellationToken))
        {
            _config = configuration;
            _apiMaster = apiMaster;
            _sharedInterviews = sharedInterviews;
            this.logger = logger;
            _cancellationToken = cancellationToken;
            WorkerId = Interlocked.Increment(ref _workerCounter).ToString();
        }

        public event EventHandler DelayChange;
        public event EventHandler StateChange;

        public string State { get; set; }

        public async Task InitQuestionnaireAsync()
        {
            await CreateInterviewAsync();
            await ConnectToSignalr().ConfigureAwait(false);

            await this._apiMaster.InitAsync(_proxy);
        }

        static readonly SemaphoreSlim StartupWindow = new SemaphoreSlim(20);

        public async Task StartAsync()
        {
            _global.Start();
            Interlocked.Increment(ref QueuedWorkers);
            await StartupWindow.WaitAsync(_cancellationToken);
            Interlocked.Decrement(ref QueuedWorkers);
            Interlocked.Increment(ref ConnectingWorkers);
            UpdateTitle();
            try
            {
                await ConnectAndAnswerQuestions();
            }
            finally
            {
                StartupWindow.Release();
                Interlocked.Increment(ref ActiveWorkers);
                Interlocked.Decrement(ref ConnectingWorkers);
            }

            UpdateTitle();

            while (!_cancellationToken.IsCancellationRequested)
            {
                await ConnectAndAnswerQuestions();
            }

            Interlocked.Decrement(ref ActiveWorkers);

            UpdateTitle();
        }

        private async Task ConnectAndAnswerQuestions()
        {
            _liveTime = Rnd.Next(_config.restartWorkersIn);

            _answersToDo = Rnd.Next(_config.questionsToAnswerRange.From ?? 0, _config.questionsToAnswerRange.To);

            if (_sharedInterviews.Any() && Rnd.NextDouble() < _config.shareInterviewPropability)
            {
                lock (_sharedInterviews)
                {
                    this.InterviewId = _sharedInterviews.Skip(Rnd.Next(0, _sharedInterviews.Count)).First();
                    Log($"Reusing other interview. #{this.InterviewId}");
                }
            }
            else
            {
                while (!await CreateInterviewAsync())
                {
                    Log("Cannot create interview");
                }

                _sharedInterviews.Add(this.InterviewId);
            }

            await ConnectToSignalr().ConfigureAwait(false);

            _restartWatcher.Restart();
            try
            {
                await QueryAsync().ConfigureAwait(false);
            }
            catch
            {
                Log("Restarting");
            }
            finally
            {
                _sharedInterviews.Remove(this.InterviewId);
                _client.Stop();
                _client.Dispose();
            }
        }

        private void Log(string message, params object[] args)
        {
            string ms = _sw == null || !_sw.IsRunning ? "" : $"({_sw.ElapsedMilliseconds} ms)";
            State = message;
            OnStateChange();
            string mess = $"#{WorkerId,-3} [{InterviewId ?? ""}]: {ms}{message}.";

            logger.Debug(mess, args);
        }

        static long ConnectedToHubCount = 0;
        static long ActiveWorkers = 0;
        static long QueuedWorkers = 0;
        static long ConnectingWorkers = 0;
        static readonly SimpleRunningAverage AvgAnswerTime = new SimpleRunningAverage(10);
        static double AverageAnswerTime = 0;
        static long ErrorsCount = 0;
        static long InterviewsCreated = 0;
        private int _answersToDo;

        private async Task ConnectToSignalr()
        {
            var signalrHub = await GetSignalrHubsUriAsync();
            _client = new HubConnection(signalrHub, $"interviewId={InterviewId}");
            _client.CookieContainer = new CookieContainer();
            _client.Closed += OnHubDisconnected;
            _proxy = _client.CreateHubProxy("interview");

            while (_client.State != ConnectionState.Connected)
                try
                {
                    _sw.Restart();
                    Log("Starting connection to Hub");
                    await _client.Start(new WebSocketTransport()).ConfigureAwait(false);
                    Log("Connected");
                }
                catch (Exception e)
                {
                    Log($"Error connecting to hub. Retry. {e.Message}");
                    await DelayAsync(_config.createInterviewDelay);
                }
                finally
                {
                    Interlocked.Increment(ref ConnectedToHubCount);
                    UpdateTitle();
                }

        }

        void UpdateTitle()
        {
            logger.Information("{totalTime} Workers - InQueue: {QueuedWorkers}, Connecting: {ConnectingWorkers}, " +
                "Active: {ActiveWorkers}. Connections to HUB: {ConnectedToHubCount}. " +
                "Avg. response: {AverageAnswerTime:F}s. Errors: {ErrorsCount}. Interviews: {InterviewsCreated} ({interviewsPerSecond:0.00} i/s)",
                _global.Elapsed.ToString(@"hh\:mm\:ss"),
                QueuedWorkers, ConnectingWorkers, ActiveWorkers, ConnectedToHubCount, AverageAnswerTime, ErrorsCount, InterviewsCreated,
                InterviewsCreated / _global.Elapsed.TotalSeconds);
        }

        private void OnHubDisconnected()
        {
            Interlocked.Decrement(ref ConnectedToHubCount);
            UpdateTitle();
        }

        private async Task<string> GetSignalrHubsUriAsync()
        {
            var pageContent = await new HttpClient().GetStringAsync(_interviewLocation);
            var signalrHubsPart = Regex.Match(pageContent, "(?:'|\")([^'^\".]*/signalr/hubs)(?:'|\")", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value.Trim('\'');
            var uriBuilder = new UriBuilder(_interviewLocation)
            {
                Path = signalrHubsPart
            };
            return uriBuilder.ToString();
        }

        private async Task QueryAsync()
        {
            try
            {
                var sw = Stopwatch.StartNew();

                while (_answersToDo > 0 && !_cancellationToken.IsCancellationRequested && _restartWatcher.ElapsedMilliseconds < _liveTime)
                {
                    await DelayAsync(Rnd.Next(_config.answerDelay));
                    _sw.Restart();

                    sw.Restart();
                    if (await _apiMaster.AnswerRandomQuestionAsync(_proxy, Rnd, logger))
                    {
                        AverageAnswerTime = AvgAnswerTime.Add(sw.Elapsed.TotalSeconds);
                        _answersToDo--;
                    }
                }
            }
            catch (Exception e)
            {
                Log($"Got exception: {e.Message}");
                Interlocked.Increment(ref ErrorsCount);
                throw;
            }
        }

        private async Task DelayAsync(int ms)
        {
            InDelay = ms;
            await Task.Delay(ms, _cancellationToken).ConfigureAwait(false);
            InDelay = null;
        }

        public int? InDelay
        {
            get => _inDelay;
            set
            {
                _inDelay = value;
                OnDelayChange();
            }
        }

        private async Task<bool> CreateInterviewAsync()
        {
            await DelayAsync(Rnd.Next(_config.createInterviewDelay));

            try
            {
                var http = new HttpClient();
                Log("Starting create interview");
                var response = await http.SendAsync(new HttpRequestMessage
                {
                    RequestUri = new Uri(_config.startUri)
                });

                _interviewLocation = response.RequestMessage.RequestUri.AbsoluteUri;
                InterviewId = GetInterviewId(_interviewLocation, "webinterview/start/") ??
                               GetInterviewId(_interviewLocation, "webinterview/");

                if (!Guid.TryParse(InterviewId, out _)) return false;

                Log("Done create interview.");
                Interlocked.Increment(ref InterviewsCreated);
                return true;
            }
            catch (Exception e)
            {
                Log($"Cannot create interview: {e.Message}");
                return false;
            }
        }


        private string GetInterviewId(string location, string marker)
        {
            // /headquarters/WebInterview/ad3d878672c945d786f247e79f3510d2/Cover
            // /headquarters/WebInterview/Start/ad3d878672c945d786f247e79f3510d2
            var webinterviewIdx = location.IndexOf(marker, StringComparison.InvariantCultureIgnoreCase);
            var interviewStart = webinterviewIdx + marker.Length;

            if (webinterviewIdx < 0) return null;
            var interviewEnd = location.IndexOf('/', interviewStart);
            if (interviewEnd == -1)
                interviewEnd = location.Length;
            return location.Substring(interviewStart, interviewEnd - interviewStart);
        }

        protected virtual void OnDelayChange()
        {
            DelayChange?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnStateChange()
        {
            StateChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
