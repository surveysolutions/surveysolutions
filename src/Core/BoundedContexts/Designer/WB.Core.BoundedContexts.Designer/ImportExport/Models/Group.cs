using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [DebuggerDisplay("Group {Id}")]
    public class Group : QuestionnaireEntity
    {
        public string? VariableName { get; set; } 
        
        public List<QuestionnaireEntity> Children { get; set; } = new List<QuestionnaireEntity>();

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}
