namespace WB.UI.WebTester.Services
{
    public class ExchangeCodeRequest
    {
        public string Code { get; set; } = "";
        public string ServiceName { get; set; } = "WB.WebTester";
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

