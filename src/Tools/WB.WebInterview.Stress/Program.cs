using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WB.WebInterview.Stress;
using YamlDotNet.Serialization;

namespace WB.WebInterview.LoadTest
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            Console.WriteLine("Hello World!");
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
            var share = new HashSet<string>();

            var logger = SetupLogger();
            await new Worker(config, apiMaster, share, logger, cts.Token).InitQuestionnaireAsync();

            for (var i = 0; i < config.workersCount; i++)
            {
                var worker = new Worker(config, apiMaster, share, logger, cts.Token);
                list.Add(Task.Run(worker.StartAsync));
            }

            Console.ReadLine();
            Console.WriteLine("Cancelling work");
            cts.Cancel();

            await Task.WhenAll(list);
        }

        static ILogger SetupLogger() => new LoggerConfiguration()
                .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
                .WriteTo.File("work.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
                .WriteTo.Sink(new ConsoleTitleSink(null))
                .CreateLogger();
    }

    public class ConsoleTitleSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly LogEventLevel restrictedToMinimumLevel;

        public ConsoleTitleSink(IFormatProvider formatProvider, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
        {
            _formatProvider = formatProvider;
            this.restrictedToMinimumLevel = restrictedToMinimumLevel;
        }

        public void Emit(LogEvent logEvent)
        {
            if(logEvent.Level >= restrictedToMinimumLevel) { 
            var message = logEvent.RenderMessage(_formatProvider);
            Console.Title = message;
                }
        }
    }
}
