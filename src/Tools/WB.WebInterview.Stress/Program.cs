using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
                    startUri = "https://superhq-dev.mysurvey.solutions/WebInterview/Start/ddb717c84a09420c9001dfb099038f1b$1",
                    createInterviewDelay = 30000,
                    workersCount = 50
                });
            }
            var file = args.Length == 0 ? "config.yml" : args[0];

            if (!File.Exists(file)) return;

            var cts = new CancellationTokenSource();
            var config = new Deserializer().Deserialize<Configuration>(File.OpenText(file));
            var list = new List<Task>();
            var apiMaster = new ApiMaster();

            for (var i = 0; i < config.workersCount; i++)
            {
                var worker = new Worker(config, apiMaster, cts.Token);
                list.Add(worker.StartAsync());
            }

            Console.ReadLine();
            Console.WriteLine("Cancelling work");
            cts.Cancel();

            Task.WaitAll(list.ToArray());
        }
    }
}
