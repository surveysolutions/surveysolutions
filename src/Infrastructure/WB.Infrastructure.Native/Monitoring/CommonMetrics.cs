namespace WB.Infrastructure.Native.Monitoring
{
    public static class CommonMetrics
    {
        public static Counter WebInterviewConnection = new Counter("wb_webinterview_connections", "Number of times connection were open", "action", "mode");
        public static Counter StatefullInterviewCached = new Counter("wb_statefullinterview_cache_create", "Total number of times statefull interview cached");
        public static Counter StatefullInterviewEvicted = new Counter("wb_statefullinterview_cache_evict", "Total number of times statefull interview cached", "reason");
        public static Counter StatefullInterviewCacheHit = new Counter("wb_statefullinterview_cache_hit", "Total number of times statefull interview cache hit");
        public static Counter StatefullInterviewCacheMiss = new Counter("wb_statefullinterview_cache_miss", "Total number of times statefull interview cache missed");

        public static Gauge NpgsqlConnections = new Gauge("npgsql_connections_current", "Number of connections managed by Npgsql", "type");
        public static Gauge NpgsqlConnectionsPoolCount = new Gauge("npgsql_connection_pools_current", "Number of connection pools managed by Npgsql");
        public static Counter NpgsqlDataCounter = new Counter("npgsql_data_total_bytes", "Amount of byte read/write by Npgsql", "direction");
    }
}
