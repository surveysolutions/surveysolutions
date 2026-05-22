namespace WB.UI.Designer.Services
{
    public class WebTesterSettings
    {
        public int ExpirationAmountMinutes { get; set; }
        public string? BaseUri { get; set; }

        /// <summary>Pre-shared secret that WebTester must present on the exchange endpoint.</summary>
        public string? ServiceApiKey { get; set; }

        /// <summary>TTL for one-time codes in seconds. Default: 60.</summary>
        public int CodeTtlSeconds { get; set; } = 60;

        /// <summary>TTL for the delegated JWT issued to WebTester in minutes.</summary>
        public int DelegatedJwtExpirationMinutes { get; set; } = 480;
    }
}
