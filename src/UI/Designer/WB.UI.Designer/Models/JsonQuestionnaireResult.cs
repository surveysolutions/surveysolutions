namespace WB.UI.Designer.Models
{
    public class JsonQuestionnaireResult : JsonSuccessResult
    {
        public string Error { get; set; }
        public bool HasPermissions { get; set; }

        public JsonQuestionnaireResult()
        {
            Error = string.Empty;
            HasPermissions = true;
        }
    }
}