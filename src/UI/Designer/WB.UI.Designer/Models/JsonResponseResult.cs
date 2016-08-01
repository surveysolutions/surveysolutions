namespace WB.UI.Designer.Models
{
    public class JsonResponseResult : JsonSuccessResult
    {
        public string Error { get; set; }
        public bool HasPermissions { get; set; }

        public JsonResponseResult()
        {
            Error = string.Empty;
            HasPermissions = true;
        }
    }
}