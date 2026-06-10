namespace WB.UI.WebTester.Services
{
    public class TesterConfiguration
    {
        public string? DesignerAddress { get; set; }
        public string? GoogleMapApiKey { get; set; }
        public string? MetricsGateway { get; set; }
        public string? InstanceName { get; set; }

        /// <summary>Pre-shared secret that matches Designer's WebTester:ServiceApiKey.</summary>
        public string? ServiceApiKey { get; set; }

        /// <summary>Service identifier sent to Designer on the exchange call. Default: WB.WebTester.</summary>
        public string? ServiceName { get; set; } = WebTesterConstants.ServiceName;
    }
}
