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
        public Group()
        {
            this.Title = string.Empty;
            this.ConditionExpression = string.Empty;
            this.Description = string.Empty;
            this.Enabled = true;
            this.Children = new List<QuestionnaireEntity>();
        }
        
        public List<QuestionnaireEntity> Children { get; set; }

        public string ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public RosterDisplayMode DisplayMode { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public bool IsRoster { get; set; }

        public bool CustomRosterTitle { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[]? FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public string Title { get; set; }
    }
}
