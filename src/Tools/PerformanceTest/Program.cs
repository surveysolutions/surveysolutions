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
            exx.TriggerRosterSizeChangeUpDown();

            switch (args.FirstOrDefault())
            {
                case "bench": BenchmarkRunner.Run<ExpressionStorageBench>(); break;
                case "trace":
                    var trace = new ExpressionStorageBench();
                    trace.IterationSetup();
                    trace.TriggerRosterSizeChangeUpDown();
                    
                    try
                    {
                        if (PerformanceProfiler.IsActive)
                        {
                            PerformanceProfiler.Begin();
                            PerformanceProfiler.Start();
                        }

                        for (int i = 0; i < 1; i++)
                        {
                            trace.TriggerRosterSizeChangeUpDown();
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
                    mem.TriggerRosterSizeChangeUpDown();

                    try
                    {
                        if (MemoryProfiler.IsActive && MemoryProfiler.CanControlAllocations)
                            MemoryProfiler.EnableAllocations();

                        //mem.SetRosterSize(60);

                        MemoryProfiler.Dump();

                        //mem.SetRosterSize(0);
                        for (int i = 0; i < 1; i++)
                        {
                            mem.TriggerRosterSizeChangeUpDown();
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