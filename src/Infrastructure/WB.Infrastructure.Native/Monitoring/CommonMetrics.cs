namespace WB.Infrastructure.Native.Monitoring
{
    public class CommonMetrics
    {
        public static readonly Gauge WebInterviewOpenConnections = new Gauge(
            "wb_hq_webinterview_connected_counter", 
            @"Number of connection to interview");

        public static readonly Gauge StateFullInterviewsCount 
            = new Gauge("wb_hq_cache_statefull_interview_counter", "Number of statefull interviews stored in HttpRuntime.Cache");

        public static readonly Counter ExceptionsLogged 
            = new Counter(@"wb_hq_exceptions_raised", @"Total exceptions raised on Headquarters");
    }
}
