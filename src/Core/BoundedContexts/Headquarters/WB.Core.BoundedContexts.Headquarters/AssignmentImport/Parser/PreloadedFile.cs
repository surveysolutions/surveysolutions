using System;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    [DebuggerDisplay("{FileName} [{Columns.Length} columns]")]
    public class PreloadedFileInfo
    {
        public string FileName { get; set; }
        public string QuestionnaireOrRosterName { get; set; }
        public string[] Columns { get; set; }
    }

    [DebuggerDisplay("{FileInfo.FileName} [{Rows.Length} rows]")]
    public class PreloadedFile
    {
        public PreloadingRow[] Rows { get; set; }

        public PreloadedFileInfo FileInfo { get; set; }
    }

    [DebuggerDisplay("[{Cells.Length} cells]")]
    public class PreloadingRow
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public PreloadingCell[] Cells { get; set; }
    }

    public abstract class PreloadingCell
    {
        public string VariableOrCodeOrPropertyName { get; set; }
    }

    [DebuggerDisplay("{ToString()}")]
    public class PreloadingValue : PreloadingCell
    {
        public int Row { get; set; }
        public string Column { get; set; }
        public string Value { get; set; }

        public override string ToString() => $"[{Row}, {Column}]={Value}";
    }

    [DebuggerDisplay("composite[{VariableOrCodeOrPropertyName}]")]
    public class PreloadingCompositeValue : PreloadingCell
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public PreloadingValue[] Values { get; set; }

        public override string ToString() =>
            $"[{Values.FirstOrDefault().Row}, {VariableOrCodeOrPropertyName}] " +
            $"values= {string.Join(", ", Values.Select(x => $"[{x.VariableOrCodeOrPropertyName}={x.Value}]"))}";
    }
}
