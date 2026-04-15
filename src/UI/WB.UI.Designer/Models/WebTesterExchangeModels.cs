namespace WB.UI.Designer.Models
{
    /// <summary>Request body for the backend-to-backend code exchange endpoint.</summary>
    public class ExchangeCodeRequest
    {
        public string Code { get; set; } = "";
        // ServiceName is NOT included here — it is passed via the X-Service-Name request header.
    }

    /// <summary>Response returned by the code exchange endpoint.</summary>
    public class ExchangeCodeResponse
    {
        public string AccessToken { get; set; } = "";
        public int ExpiresIn { get; set; }
        public string? UserId { get; set; }
        public string CorrelationId { get; set; } = "";
        public string QuestionnaireId { get; set; } = "";
    }
}

