using System.Collections.Generic;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    public class PostgresSystemColumns
    {
        static HashSet<string> systemColumns = new HashSet<string>()
        {
            "oid",
            "tableoid",
            "xmin",
            "cmin",
            "xmax",
            "cmax",
            "ctid",
        };

        public static string Escape(string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return columnName;
            if (systemColumns.Contains(columnName.ToLower()))
                return columnName + "__1";
            return columnName;
        }
    }
}
