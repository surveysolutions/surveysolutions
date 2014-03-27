namespace WB.UI.Headquarters.Utils
{
    public static class Alerts
    {
        public const string ATTENTION = "attention";

        public const string ERROR = "error";

        public const string INFORMATION = "info";

        public const string SUCCESS = "success";

        public static string[] ALL
        {
            get { return new[] { SUCCESS, ATTENTION, INFORMATION, ERROR }; }
        }
    }
}