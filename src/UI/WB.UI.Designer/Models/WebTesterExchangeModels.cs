using System.ComponentModel.DataAnnotations;

namespace WB.UI.Designer.Models
{
    /// <summary>Request body for the backend-to-backend code exchange endpoint.</summary>
    public class ExchangeCodeRequest
    {
        /// <summary>
        /// One-time code issued by Designer.  Codes are 43-character base64url strings
        /// (32 random bytes, no padding).  A ceiling of 128 characters is enforced so
        /// that over-length values are rejected before they reach the cache layer.
        /// </summary>
        [MaxLength(128, ErrorMessage = "Code must not exceed 128 characters")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$",
            ErrorMessage = "Code contains invalid characters (expected base64url: A-Z a-z 0-9 _ -)")]
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

