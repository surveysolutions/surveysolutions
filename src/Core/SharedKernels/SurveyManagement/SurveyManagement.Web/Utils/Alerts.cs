namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils
{
    public static class Alerts
    {
        
        public const string ATTENTION = "warning";
        public const string ERROR = "danger";
        public const string INFORMATION = "info";
        public const string SUCCESS = "success";

        public static string[] ALL
        {
            get
            {
                return new[] { SUCCESS, ATTENTION, INFORMATION, ERROR };
            }
        }
    }
}
