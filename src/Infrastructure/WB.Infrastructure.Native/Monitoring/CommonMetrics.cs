namespace WB.Infrastructure.Native.Monitoring
{
    public class CommonMetrics
    {
        public static Gauge StateFullInterviewsCount = new Gauge(
            "wb_hq_cache_statefull_interview_counter", "Number of statefull interviews stored in HttpRuntime.Cache");
    }
}
