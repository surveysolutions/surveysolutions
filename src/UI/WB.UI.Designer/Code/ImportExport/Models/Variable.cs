using System.Diagnostics;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [DebuggerDisplay("Variable {PublicKey}")]
    public class Variable : QuestionnaireEntity
    {
        public string? Label { get; set; }
        public VariableType Type { get; set; }
        public string? Name { get; set; }
        public string? Expression { get; set; }
        public bool DoNotExport { get; set; }
    }
}
