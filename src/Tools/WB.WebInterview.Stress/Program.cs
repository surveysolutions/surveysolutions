using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using YamlDotNet.Serialization;

namespace WB.WebInterview.Stress
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 && !File.Exists("config.yml"))
            {
                Console.WriteLine("Usage .exe {configuration}.yml.");
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
                var config = new Deserializer().Deserialize<Configuration>(File.OpenText(file));
                var factory = new WorkerFactory(config);
                var list = new List<Task>();

                for (int i = 0; i < config.workersCount; i++)
                {
                    var worker = factory.Create();
                    list.Add(worker.Start());
                }

                Task.WaitAll(list.ToArray());
            }
        }

        public class Configuration
        {
            public string baseUri { get; set; }
            public int workersCount { get; set; }
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

            public Worker Create()
            {
                var worker = new Worker(_config.baseUri, _config.questionarieId, CancellationToken.None);
                worker.SetDelay(_config.answerDelay, _config.createInterviewDelay);

                foreach (var answer in _config.answers)
                {
                    worker.AddAnswer(answer[0], answer.Skip(1).ToArray());
                }

                return worker;
            }
        }

        [Localizable(false)]
        public class Worker
        {
            private readonly string _baseUri;
            private readonly string _questionarieId;
            private readonly CancellationToken _cancellationToken;
            private string _interviewId;
            private readonly string _workerId;
            private static long _workerCounter = 0;
            private readonly List<Tuple<string, string[]>> _answers = new List<Tuple<string, string[]>>();
            private int _answeringDelay = 2500;
            private int _createInterviewDelay = 30000;

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

            public void SetDelay(int answeringDelay = 2500, int createInterviewDelay = 30000)
            {
                _answeringDelay = answeringDelay;
                _createInterviewDelay = createInterviewDelay;
            }

            public async Task Start()
            {
                while (!await CreateInterview()) ;
                if (_interviewId == "Start") return;
                await Work();
            }

            static Random _rnd = new Random();

            private void Log(string message, Stopwatch sw = null)
            {
                Console.WriteLine($"#{_workerId, -3} [{_interviewId ?? ""}]: {message}." + (sw == null ? "" : $"({sw.ElapsedMilliseconds} ms)"), "Worker");
            }

            private async Task Work()
            {
                var client = new HubConnection($"{_baseUri}/signalr/hubs", $"interviewId={_interviewId}");
                var proxy = client.CreateHubProxy("interview");

                var sw = new Stopwatch();
                while (client.State != ConnectionState.Connected)
                {
                    try
                    {
                        await Task.Delay(this._createInterviewDelay);

                        sw.Restart();
                        Log("Starting connection to Hub");
                        await client.Start(new WebSocketTransport());
                        Log("Connected", sw);
                    }
                    catch (Exception e)
                    {
                        Log("Error connecting to hub. Retry. " + e.Message, sw);
                    }
                }

                while (!_cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(_rnd.Next(_answeringDelay), _cancellationToken);
                    var answer = _rnd.Next(10000).ToString();
                    sw.Restart();
                    await InvokeRandomCommand(proxy, sw);
                }
            }

            Tuple<string, string[]> _lastCommand = null;

            private async Task InvokeRandomCommand(IHubProxy proxy, Stopwatch sw)
            {
                Tuple<string, string[]> command;

                do
                {
                    command = _answers[_rnd.Next(0, _answers.Count - 1)];
                } while (Equals(command, _lastCommand));

                _lastCommand = command;
                try
                {
                    await proxy.Invoke(command.Item1, command.Item2);
                    Log($"Invoked, {command.Item1} ({string.Join(", ", command.Item2)})", sw);
                }
                catch (Exception e)
                {
                    Log($"Cannot invoke, {command.Item1} ({string.Join(", ", command.Item2)}): {e.Message}", sw);
                }
            }

            private async Task<bool> CreateInterview()
            {
                await Task.Delay(_rnd.Next(_createInterviewDelay));

                var sw = new Stopwatch();
                sw.Start();

                try
                {
                    var http = new HttpClient();
                    Log("Starting create interview");
                    var startInterviewPage =
                        await http.GetStringAsync($"{_baseUri}/WebInterview/Start/{_questionarieId}");

                    var forgeryToken = GetAntiForgeryToken(startInterviewPage);
                    http.DefaultRequestHeaders.Add("Cookie", $"__RequestVerificationToken={forgeryToken}");

                    var response = await http.PostAsync($"{_baseUri}/WebInterview/Start/{_questionarieId}",
                        new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                        {
                            new KeyValuePair<string, string>("__RequestVerificationToken", forgeryToken)
                        }));

                    var interviewLocation = response.RequestMessage.RequestUri.AbsoluteUri;
                    _interviewId = GetInterviewId(interviewLocation, "webinterview/start/") ??
                                   GetInterviewId(interviewLocation, "webinterview/");

                    Log("Done create interview.", sw);
                    return true;
                }
                catch (Exception e)
                {
                    Log("Cannot create interview: " + e.Message, sw);
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
                {
                    interviewEnd = location.Length;
                }
                return location.Substring(interviewStart, interviewEnd - interviewStart);
            }
        }
    }
}
