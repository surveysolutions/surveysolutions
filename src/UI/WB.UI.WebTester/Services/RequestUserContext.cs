namespace WB.UI.WebTester.Services
{
    public sealed class RequestUserContext
    {
        public string? UserId { get; init; }
        public string? CorrelationId { get; init; }
        public string? DelegatedToken { get; init; }
    }
}

