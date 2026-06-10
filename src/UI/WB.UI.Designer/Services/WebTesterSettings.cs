namespace WB.UI.Designer.Services
{
    public class WebTesterSettings
    {
        public int ExpirationAmountMinutes { get; set; }
        public string? BaseUri { get; set; }

        /// <summary>Pre-shared secret that WebTester must present on the exchange endpoint.</summary>
        public string? ServiceApiKey { get; set; }

        /// <summary>
        /// Secret key used to sign and verify delegated JWTs issued to WebTester.
        /// Must be at least 32 characters. Must match the value used by WebTester.
        /// When absent, the Run-in-WebTester feature will return a configuration error.
        /// </summary>
        public string? JwtSecretKey { get; set; }

        /// <summary>TTL for one-time codes in seconds. Default: 60.</summary>
        public int CodeTtlSeconds { get; set; } = 60;

        /// <summary>TTL for the delegated JWT issued to WebTester in minutes.</summary>
        public int DelegatedJwtExpirationMinutes { get; set; } = 480;
    }
}
