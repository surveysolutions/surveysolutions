namespace WB.UI.WebTester.Services
{
    /// <summary>
    /// Tracing/audit context for a WebTester interview session.
    /// Contains only non-secret identifiers — the delegated JWT is stored separately
    /// in <see cref="IWebTesterJwtStore"/> (which enforces TTL-based expiry) and is
    /// never duplicated here.
    /// </summary>
    public sealed class RequestUserContext
    {
        public string? UserId { get; init; }
        public string? CorrelationId { get; init; }
    }
}
