using System.Diagnostics;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [DebuggerDisplay("Variable {PublicKey}")]
    public class Variable : QuestionnaireEntity
    {
        public string? Label { get; set; }
        public VariableType VariableType { get; set; }
        public string? VariableName { get; set; }
        public string? Expression { get; set; }
        public bool DoNotExport { get; set; }
    }
}
