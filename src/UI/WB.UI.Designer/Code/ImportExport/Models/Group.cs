using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : IQuestionnaireEntity
    {
        public Group()
        {
            this.Title = string.Empty;
            this.ConditionExpression = string.Empty;
            this.Description = string.Empty;
            this.Enabled = true;
            this.FixedRosterTitles = new FixedRosterTitle[0];
            this.Children = new List<IQuestionnaireEntity>();
        }
        
        public List<IQuestionnaireEntity> Children { get; set; }

        public string ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public RosterDisplayMode DisplayMode { get; set; }

        public bool Enabled { get; set; }

        public string Description { get; set; }

        public string VariableName { get; set; } = String.Empty;

        public bool IsRoster { get; set; }

        public bool CustomRosterTitle { get; set; }

        public Guid? RosterSizeQuestionId { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public Guid? RosterTitleQuestionId { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }
    }
}
