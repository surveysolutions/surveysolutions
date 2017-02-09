using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using YamlDotNet.Serialization;

namespace WB.WebInterview.Stress
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0 && !File.Exists("config.yml"))
            {
                Console.WriteLine(
                    $"Sample usage {Process.GetCurrentProcess().ProcessName}.exe config.yml.");
                Console.WriteLine("");
                Console.WriteLine("Sample config:");

                new Serializer().Serialize(Console.Out, new Configuration
                {
                    baseUri = "http://localhost/headquarters",
                    questionarieId = "c9805a29d40e4939af33cf3dd6b1a969$21",
                    createInterviewDelay = 30000,
                    workersCount = 50,
                    answers = new[]
                    {
                        new[] {"answerSingleOptionQuestion", "1", "eb8d67cdb968a79dcb009708b1132b94"},
                        new[] {"answerSingleOptionQuestion", "2", "eb8d67cdb968a79dcb009708b1132b94"},
                        new[] {"answerSingleOptionQuestion", "1", "8e7d17c35c4f56cd37d8b7fcceedae7f"},
                        new[] {"answerSingleOptionQuestion", "2", "8e7d17c35c4f56cd37d8b7fcceedae7f"}
                    }
                });
            }
            var file = args.Length == 0 ? "config.yml" : args[0];

            if (File.Exists(file))
            {
                var cts = new CancellationTokenSource();
                var config = new Deserializer().Deserialize<Configuration>(File.OpenText(file));
                var factory = new WorkerFactory(config);
                var list = new List<Task>();

                for (var i = 0; i < config.workersCount; i++)
                {
                    var worker = factory.Create(cts.Token);
                    list.Add(worker.StartAsync());
                }

                Console.ReadLine();
                Console.WriteLine("Cancelling work");
                cts.Cancel();

                Task.WaitAll(list.ToArray());
            }
        }

        public class Configuration
        {
            public string baseUri { get; set; }
            public int workersCount { get; set; }
            public int restartWorkersIn { get; set; } = 30000;
            public string questionarieId { get; set; }
            public int answerDelay { get; set; } = 1000;
            public int createInterviewDelay { get; set; } = 30000;
            public string[][] answers { get; set; }
        }

        public class WorkerFactory
        {
            private readonly Configuration _config;
            private readonly string _filename;

            public WorkerFactory(Configuration config)
            {
                _config = config;
            }

            public Worker Create(CancellationToken ctsToken)
            {
                var worker = new Worker(_config.baseUri, _config.questionarieId, ctsToken);
                worker.SetDelay(_config.answerDelay, _config.createInterviewDelay, _config.restartWorkersIn);

                foreach (var answer in _config.answers)
                    worker.AddAnswer(answer[0], answer.Skip(1).ToArray());

                return worker;
            }
        }

        [Localizable(false)]
        public class Worker
        {
            private static long _workerCounter;

            private static readonly Random _rnd = new Random();
            private readonly List<Tuple<string, string[]>> _answers = new List<Tuple<string, string[]>>();
            private readonly string _baseUri;
            private readonly CancellationToken _cancellationToken;
            private readonly string _questionarieId;
            private readonly string _workerId;
            private readonly Stopwatch restartWatcher = new Stopwatch();

            private readonly Stopwatch sw = new Stopwatch();
            private int _answeringDelay = 2500;
            private HubConnection _client;
            private int _configRestartWorkersIn;
            private int _createInterviewDelay = 30000;
            private string _interviewId;

            private Tuple<string, string[]> _lastCommand;
            private IHubProxy _proxy;
            private int liveTime;

            public Worker(string baseUri, string questionarieId, CancellationToken cancellationToken)
            {
                _baseUri = baseUri;
                _questionarieId = questionarieId;
                _cancellationToken = cancellationToken;
                _workerId = Interlocked.Increment(ref _workerCounter).ToString();
            }

            public void AddAnswer(string actionName, params string[] args)
            {
                _answers.Add(Tuple.Create(actionName, args));
            }

            public void SetDelay(int answeringDelay = 2500, int createInterviewDelay = 30000,
                int configRestartWorkersIn = 30000)
            {
                _answeringDelay = answeringDelay;
                _createInterviewDelay = createInterviewDelay;
                _configRestartWorkersIn = configRestartWorkersIn;
            }

            public async Task StartAsync()
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    liveTime = _rnd.Next(_configRestartWorkersIn);

                    while (!await CreateInterviewAsync()) ;

                    await WorkAsync().ConfigureAwait(false);

                    restartWatcher.Restart();
                    await QueryAsync().ConfigureAwait(false);
                    _client.Stop();
                    _client.Dispose();
                }
            }

            private void Log(string message)
            {
                Console.WriteLine(
                    $"#{_workerId,-3} [{_interviewId ?? ""}]: {message}." +
                    (sw == null || !sw.IsRunning ? "" : $"({sw.ElapsedMilliseconds} ms)"), "Worker");
            }

            private async Task WorkAsync()
            {
                _client = new HubConnection($"{_baseUri}/signalr/hubs", $"interviewId={_interviewId}");
                _proxy = _client.CreateHubProxy("interview");

                while (_client.State != ConnectionState.Connected)
                    try
                    {
                        await Task.Delay(_createInterviewDelay, _cancellationToken).ConfigureAwait(false);

                        sw.Restart();
                        Log("Starting connection to Hub");
                        await _client.Start(new WebSocketTransport()).ConfigureAwait(false);
                        Log("Connected");
                    }
                    catch (Exception e)
                    {
                        Log($"Error connecting to hub. Retry. {e.Message}");
                    }
            }

            private async Task QueryAsync()
            {
                try
                {
                    while (!_cancellationToken.IsCancellationRequested && restartWatcher.ElapsedMilliseconds < liveTime)
                    {
                        await Task.Delay(_rnd.Next(_answeringDelay), _cancellationToken).ConfigureAwait(false);
                        sw.Restart();
                        if (!await InvokeRandomCommandAsync().ConfigureAwait(false)) return;
                    }
                }
                catch (Exception e)
                {
                    Log($"Got exception: {e.Message}");
                    throw;
                }
            }

            private async Task<bool> InvokeRandomCommandAsync()
            {
                Tuple<string, string[]> command;

                do
                {
                    command = _answers[_rnd.Next(0, _answers.Count - 1)];
                } while (Equals(command, _lastCommand));

                _lastCommand = command;
                try
                {
                    await _proxy.Invoke(command.Item1, command.Item2).ConfigureAwait(false);
                    Log($"Invoked, {command.Item1} ({string.Join(", ", command.Item2)})");
                    return true;
                }
                catch (Exception e)
                {
                    Log($"Cannot invoke, {command.Item1} ({string.Join(", ", command.Item2)}): {e.Message}");
                    return false;
                }
            }

            private async Task<bool> CreateInterviewAsync()
            {
                await Task.Delay(_rnd.Next(_createInterviewDelay), _cancellationToken).ConfigureAwait(false);

                try
                {
                    var http = new HttpClient();
                    Log("Starting create interview");
                    var startInterviewPage = await http.GetStringAsync($"{_baseUri}/WebInterview/Start/{_questionarieId}");

                    var forgeryToken = GetAntiForgeryToken(startInterviewPage);
                    http.DefaultRequestHeaders.Add("Cookie", $"__RequestVerificationToken={forgeryToken}");

                    var response = await http.PostAsync($"{_baseUri}/WebInterview/Start/{_questionarieId}",
                        new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("__RequestVerificationToken", forgeryToken),
                            new KeyValuePair<string, string>("resume", "false")
                        }), _cancellationToken);

                    var text = await response.Content.ReadAsStringAsync();
                    var interviewLocation = response.RequestMessage.RequestUri.AbsoluteUri;
                    _interviewId = GetInterviewId(interviewLocation, "webinterview/start/") ??
                                   GetInterviewId(interviewLocation, "webinterview/");

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
        }
    }
}