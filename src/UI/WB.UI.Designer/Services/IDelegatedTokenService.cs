using System;

namespace WB.UI.Designer.Services
{
    public class DelegatedTokenRequest
    {
        public string? UserId { get; init; }
        public string CorrelationId { get; init; } = "";
        public Guid QuestionnaireId { get; init; }
        public string AuthorizedParty { get; init; } = WebTesterConstants.ServiceName;
        public string Scope { get; init; } = "webtester";
    }

    public interface IDelegatedTokenService
    {
        string CreateDelegatedToken(DelegatedTokenRequest request);
    }
}

