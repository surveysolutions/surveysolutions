using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;

namespace PerformanceTest
{
    public class MonitoringConfig : ManualConfig
    {
        public MonitoringConfig()
        {
            this.Add(Job.ShortRun
                .With(Runtime.Clr)
                .With(Jit.RyuJit)
                .WithTargetCount(8)
                .With(Platform.X64)
                .WithId("NET4.7_RyuJIT-x64"));

            //this.Add(RPlotExporter.Default);
            this.Add(MarkdownExporter.Default);
            this.Add(MemoryDiagnoser.Default);
            
            this.KeepBenchmarkFiles = true;
        }
    }
}