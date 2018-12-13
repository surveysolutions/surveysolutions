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

        public object GetData(int rowIndex, string columnName)
        {
            if (rowIndex < 0 || rowIndex > Data.Length)
                return null;

            if (string.IsNullOrWhiteSpace(columnName))
                return null;

            var columnIndex = System.Array.IndexOf(Headers, columnName);
            if (columnIndex < 0)
                return null;

            return Data[rowIndex][columnIndex];
        }
    }
}
