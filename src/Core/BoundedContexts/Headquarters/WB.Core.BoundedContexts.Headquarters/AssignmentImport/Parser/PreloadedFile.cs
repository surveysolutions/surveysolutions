using System.Diagnostics;
using System.Linq;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    [DebuggerDisplay("{FileName} [{Rows.Length} rows]")]
    public class PreloadedFile
    {
        public string FileName { get; set; }
        public string QuestionnaireOrRosterName { get; set; }

        public string[] Columns { get; set; }
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public PreloadingRow[] Rows { get; set; }
    }

    [DebuggerDisplay("{InterviewId ?? \"No interview id\"} [{Cells.Length} cells]")]
    public class PreloadingRow
    {
        public string InterviewId { get; set; }
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public PreloadingCell[] Cells { get; set; }
    }

    public abstract class PreloadingCell
    {
        public string VariableOrCodeOrPropertyName { get; set; }
    }

    [DebuggerDisplay("[{Row}, {Column}] value='{Value}'")]
    public class PreloadingValue : PreloadingCell
    {
        public int Row { get; set; }
        public string Column { get; set; }
        public string Value { get; set; }
    }

    [DebuggerDisplay("{ToString()}")]
    public class PreloadingCompositeValue : PreloadingCell
    {
        public PreloadingValue[] Values { get; set; }

        public override string ToString() =>
            $"[{Values.FirstOrDefault().Row}, {VariableOrCodeOrPropertyName}] " +
            $"values= {string.Join(", ", Values.Select(x => $"[{x.VariableOrCodeOrPropertyName}={x.Value}]"))}";
    }
}