using System;

namespace WB.UI.Designer.Services
{
    /// <summary>
    /// Represents a one-time authorization code used to securely bootstrap a WebTester session
    /// without transmitting the user JWT through the browser.
    /// </summary>
    public class OneTimeCodeEntity
    {
        public string Code { get; init; } = "";
        public string? UserId { get; init; }
        public string CorrelationId { get; init; } = "";
        public string TargetService { get; init; } = "";
        public Guid QuestionnaireId { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime ExpiresAt { get; init; }
        public bool Used { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}

