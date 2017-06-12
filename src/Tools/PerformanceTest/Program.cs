using System.Linq;
using BenchmarkDotNet.Running;
using JetBrains.Profiler.Windows.Api;


namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // make sure it fail before tests run
            var exx = new ExpressionStorageBench();
            exx.IterationSetup();
            exx.AnswerSimpleTextQuestion();
            exx.TriggerRosterByAnswerSingleOptionQuestion();
            exx.TriggerRosterSizeChange();

            switch (args.FirstOrDefault())
            {
                case "bench": BenchmarkRunner.Run<ExpressionStorageBench>(); break;
                case "trace":
                    var trace = new ExpressionStorageBench();
                    trace.IterationSetup();
                    trace.TriggerRosterSizeChange();
                    trace.TriggerRosterSizeChange();
                    trace.TriggerRosterSizeChange();
                    trace.TriggerRosterSizeChange();

                    try
                    {
                        if (PerformanceProfiler.IsActive)
                        {
                            PerformanceProfiler.Begin();
                            PerformanceProfiler.Start();
                        }

                        for (int i = 0; i < 10; i++)
                        {
                            trace.TriggerRosterSizeChange();
                        }
                    }
                    finally
                    {
                        PerformanceProfiler.Stop();
                        PerformanceProfiler.EndSave();
                    }
                    break;
                case "memory":
                    var mem = new ExpressionStorageBench();
                    mem.IterationSetup();
                    mem.TriggerRosterSizeChange();

                    try
                    {
                        if (MemoryProfiler.IsActive && MemoryProfiler.CanControlAllocations)
                            MemoryProfiler.EnableAllocations();

                        MemoryProfiler.Dump();

                        for (int i = 0; i < 10; i++)
                        {
                            mem.TriggerRosterSizeChange();
                        }
                    }
                    finally
                    {
                        MemoryProfiler.Dump();
                    }

                    break;
            }
        }
    }
}