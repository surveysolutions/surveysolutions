using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using WB.UI.Designer.Code.ImportExport.Models.Question;

namespace WB.UI.Designer.Code.ImportExport.Models
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
