namespace WB.UI.Shared.Web.Extensions
{
    public static class Alerts
    {
        public const string ATTENTION = "warning";
        public const string ERROR = "danger";
        public const string INFORMATION = "info";
        public const string SUCCESS = "success";

        public static string[] ALL => new[] { SUCCESS, ATTENTION, INFORMATION, ERROR };
    }
}
