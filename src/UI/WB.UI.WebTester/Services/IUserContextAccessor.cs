namespace WB.UI.WebTester.Services
{
    public interface IUserContextAccessor
    {
        string? UserId { get; }
        string? CorrelationId { get; }
        string? AccessToken { get; }
        void SetContext(RequestUserContext context);
    }

    /// <summary>
    /// Scoped per-request holder for delegated user context extracted from
    /// the code-exchange response.
    /// </summary>
    public class UserContextAccessor : IUserContextAccessor
    {
        private RequestUserContext? context;

        public string? UserId => context?.UserId;
        public string? CorrelationId => context?.CorrelationId;
        public string? AccessToken => context?.DelegatedToken;

        public void SetContext(RequestUserContext value) => context = value;
    }
}
