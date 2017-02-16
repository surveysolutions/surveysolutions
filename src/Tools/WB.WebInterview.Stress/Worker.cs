using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

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

        public Worker(Configuration configuration, ApiMaster apiMaster, HashSet<string> sharedInterviews, CancellationToken cancellationToken = default(CancellationToken))
        {
            _config = configuration;
            _apiMaster = apiMaster;
            _sharedInterviews = sharedInterviews;
            _cancellationToken = cancellationToken;
            WorkerId = Interlocked.Increment(ref _workerCounter).ToString();
        }

        public event EventHandler DelayChange;
        public event EventHandler StateChange;

        public string State { get; set; }

        public async Task StartAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                _liveTime = Rnd.Next(_config.restartWorkersIn);

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

                await WorkAsync().ConfigureAwait(false);

                await this._apiMaster.InitAsync(_proxy);

                _restartWatcher.Restart();
                await QueryAsync().ConfigureAwait(false);

                _sharedInterviews.Remove(this.InterviewId);
                _client.Stop();
                _client.Dispose();
            }
        }

        private void Log(string message)
        {
            string ms = _sw == null || !_sw.IsRunning ? "" : $"({_sw.ElapsedMilliseconds} ms)";
            State = message;
            OnStateChange();
            string mess = $"#{WorkerId,-3} [{InterviewId ?? ""}]: {ms}{message}.";
            Console.WriteLine(mess);
        }

        private async Task WorkAsync()
        {
            var signalrHub = await GetSignalrHubsUriAsync();
            _client = new HubConnection(signalrHub, $"interviewId={InterviewId}");

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
                while (!_cancellationToken.IsCancellationRequested && _restartWatcher.ElapsedMilliseconds < _liveTime)
                {
                    await DelayAsync(Rnd.Next(_config.answerDelay));
                    _sw.Restart();

                    await _apiMaster.AnswerRandomQuestionAsync(_proxy, Rnd, Log);
                }
            }
            catch (Exception e)
            {
                Log($"Got exception: {e.Message}");
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
            get { return _inDelay; }
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
                var startInterviewPage = await http.GetStringAsync(_config.startUri);

                var forgeryToken = GetAntiForgeryToken(startInterviewPage);
                http.DefaultRequestHeaders.Add("Cookie", $"__RequestVerificationToken={forgeryToken}");

                var response = await http.PostAsync(_config.startUri, new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("__RequestVerificationToken", forgeryToken),
                        new KeyValuePair<string, string>("resume", "false")
                    }), _cancellationToken);

                _interviewLocation = response.RequestMessage.RequestUri.AbsoluteUri;
                InterviewId = GetInterviewId(_interviewLocation, "webinterview/start/") ??
                               GetInterviewId(_interviewLocation, "webinterview/");

                Guid interviewId;
                if (!Guid.TryParse(InterviewId, out interviewId)) return false;

                Log("Done create interview.");
                return true;
            }
            catch (Exception e)
            {
                Log($"Cannot create interview: {e.Message}");
                return false;
            }
        }

        private string GetAntiForgeryToken(string page)
        {
            // <input name="__RequestVerificationToken" type="hidden" value="P2Y94wdSa9ocMyaUDyCLmZNQ3TDzYzmpdmFSVmuRR2Quw2pQdpFiN6NB6rRdiDaktA5V0lkAp1a0SsdsiDE-Baha85DpofkAt1Y7PFA439s1" />
            var tokenIdx = page.IndexOf("__RequestVerificationToken", StringComparison.InvariantCultureIgnoreCase);
            var valueStart = page.IndexOf("value=\"", tokenIdx, StringComparison.InvariantCultureIgnoreCase) +
                             "value=\"".Length;
            var valueEnd = page.IndexOf('"', valueStart);
            return page.Substring(valueStart, valueEnd - valueStart);
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