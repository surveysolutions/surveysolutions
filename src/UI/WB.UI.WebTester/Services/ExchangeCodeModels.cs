namespace WB.UI.WebTester.Services
{
    public class ExchangeCodeRequest
    {
        public string Code { get; set; } = "";
        // ServiceName is NOT part of the request body — it is passed via the X-Service-Name header.
    }

    public class ExchangeCodeResponse
    {
        public string AccessToken { get; set; } = "";
        public int ExpiresIn { get; set; }
        public string? UserId { get; set; }
        public string CorrelationId { get; set; } = "";
        public string QuestionnaireId { get; set; } = "";
    }
}

