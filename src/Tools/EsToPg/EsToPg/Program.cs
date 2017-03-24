using System;
using System.Data;
using System.IO;
using System.Reflection;
using NConsole;

namespace EsToPg
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor(new ConsoleHost());
            processor.RegisterCommand<MigCommand>("mig");
            processor.Process(args);
        }
    }

    internal class Event
    {
        public Guid Id { get; set; }
        public string Origin { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid EventSourceId { get; set; }
        public long GlobalSequence { get; set; }
        public long EventSequence { get; set; }
        public string Value { get; set; }
        public string EventType { get; set; }
    }
}
