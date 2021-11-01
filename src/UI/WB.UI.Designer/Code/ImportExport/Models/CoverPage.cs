using System.Collections.Generic;
using System.Diagnostics;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [DebuggerDisplay("CoverPage {Id}")]
    public class CoverPage : QuestionnaireEntity
    {
        public string Title { get; set; } = string.Empty;

        public string? VariableName { get; set; } 
        
        public List<QuestionnaireEntity> Children { get; set; } = new List<QuestionnaireEntity>();
    }
}