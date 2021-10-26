using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using WB.UI.Designer.Code.ImportExport.Models.Question;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : QuestionnaireEntity
    {
        public string? VariableName { get; set; } 
        
        public List<QuestionnaireEntity> Children { get; set; } = new List<QuestionnaireEntity>();

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public RosterDisplayMode DisplayMode { get; set; }

        //public bool Enabled { get; set; } = true;

        public string Description { get; set; } = string.Empty;

        public bool IsRoster { get; set; }

        public bool CustomRosterTitle { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[]? FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public string Title { get; set; } = string.Empty;
    }
}
