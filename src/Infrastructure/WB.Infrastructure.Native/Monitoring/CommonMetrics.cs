
namespace WB.Infrastructure.Native.Monitoring
{
    public static class CommonMetrics
    {
        public static Counter WebInterviewOpenConnection  = new Counter("wb_webinterview_connection_open",  "Number of times connection were open", "mode");
        public static Counter WebInterviewCloseConnection = new Counter("wb_webinterview_connection_close", "Number of times connection were open", "mode");
        public static Counter StatefullInterviewCached    = new Counter("wb_statefullinterview_cache_create", "Total number of times statefull interview cached");
        public static Counter StatefullInterviewEvicted   = new Counter("wb_statefullinterview_cache_evict", "Total number of times statefull interview cached", "reason");
        public static Counter StatefullInterviewCacheHit = new Counter("wb_statefullinterview_cache_hit", "Total number of times statefull interview cache hit");
        public static Counter StatefullInterviewCacheMiss  = new Counter("wb_statefullinterview_cache_miss", "Total number of times statefull interview cache missed");
    }
}
