#nullable enable
namespace WB.Core.Infrastructure.Metrics
{
    public static class CoreMetrics
    {
        public static ICounter? StatefullInterviewsCached = null;
        public static ICounter? StatefullInterviewCacheHit = null;
        public static ICounter? StatefullInterviewCacheMiss = null;
    }
}
