namespace WB.UI.Headquarters.Configs
{
    public class MetricsConfig
    {
        public string Endpoint { get; set; } = "http://localhost:9001";
        public bool UsePushGateway { get; set; } = true;
        public bool UseMetricsEndpoint { get; set; } = false;
    }
}
