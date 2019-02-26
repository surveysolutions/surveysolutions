using System.Diagnostics;
using NpgsqlTypes;

namespace WB.Services.Export.InterviewDataStorage.InterviewDataExport
{
    [DebuggerDisplay("{" + nameof(ToString) + "()}")]
    public class UpdateValueInfo
    {
        public string ColumnName { get; set; }
        public object Value { get; set; }
        public NpgsqlDbType ValueType { get; set; }

        public override string ToString() => ColumnName + "-" + Value + "-" + ValueType;
    }
}
