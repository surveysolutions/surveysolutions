namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class ReportView
    {
        public string Name { get; set; }
        public object[] Totals { get; set; }
        public string[] Columns { get; set; }
        public string[] Headers { get; set; }
        public object[][] Data { get; set; }
        public long TotalCount { get; set; }

        public enum ColumnType
        {
            Text, Numeric, Total
        }
    }
}
