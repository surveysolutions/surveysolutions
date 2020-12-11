using WB.Core.Infrastructure.Metrics;

namespace WB.Infrastructure.Native.Monitoring
{
    public static class CommonMetrics
    {
        static CommonMetrics()
        {
            CoreMetrics.StatefullInterviewCacheHit = new Counter("wb_statefullinterview_cache_hit", "Total number of times statefull interview cache hit");
            CoreMetrics.StatefullInterviewsCached = new Counter("wb_statefullinterview_cached_total", "Total number of times statefull interview cached (added/removed)", "action");
            CoreMetrics.StatefullInterviewCacheMiss = new Counter("wb_statefullinterview_cache_miss", "Total number of times statefull interview cache missed");
        }

        public static Counter WebInterviewConnection = new Counter("wb_webinterview_connections_total", 
            "Number of times connection were open (open/closed), (review,takenew,web)", "action", "mode", "workspace");
        
        public static Gauge NpgsqlConnections = new Gauge("npgsql_connections_current", "Number of connections managed by Npgsql (idle/busy)", "type");
        public static Gauge NpgsqlConnectionsPoolCount = new Gauge("npgsql_connection_pools_current", "Number of connection pools managed by Npgsql");
        public static Counter NpgsqlDataCounter = new Counter("npgsql_data_bytes_total", "Amount of byte read/write by Npgsql", "direction");
        public static Counter ExceptionsOccur = new Counter("wb_exceptions_total", "Total number of exceptions happen on site during it's life time");

        public static Counter InterviewsCreatedCount = new Counter("wb_interviews_created_total", "Total number of interviews created during app life time");
        public static Counter EventsCreatedCount = new Counter("wb_events_created_total", "Total number of events created during app life time");
    }
}
