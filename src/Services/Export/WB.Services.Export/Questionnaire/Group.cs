using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Services.Export.Questionnaire
{
    public enum RosterSizeSourceType
    {
        Question = 0,
        FixedTitles = 1
    }

    [DebuggerDisplay("Group {PublicKey}")]
    public class Group : IQuestionnaireEntity
    {
        public Group(List<IQuestionnaireEntity> children = null)
        {
            if (children == null)
                this.Children = new List<IQuestionnaireEntity>();
            else
            {
                this.Children = children;
                this.ConnectChildrenWithParent();
            }
        }

        public virtual bool IsRoster { get; set; }

        public Guid? RosterSizeQuestionId { get; set;  }

        public RosterSizeSourceType RosterSizeSource { get; set;  }

        public bool IsFixedRoster => this.IsRoster && this.RosterSizeSource == RosterSizeSourceType.FixedTitles;

        public IQuestionnaireEntity Parent { get; private set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; } = Array.Empty<FixedRosterTitle>();
        
        public string VariableName { get; set;  }

        public string Title { get; set;  }

        public Guid? RosterTitleQuestionId { get; set;  }

        public Guid PublicKey { get; set;  }

        public IEnumerable<IQuestionnaireEntity> Children { get; set; } = new List<IQuestionnaireEntity>();

        public IQuestionnaireEntity GetParent()
        {
            return Parent;
        }

        public void SetParent(IQuestionnaireEntity parent)
        {
            this.Parent = parent;
            foreach (var questionnaireEntity in Children)
            {
                questionnaireEntity.SetParent(this);
            }
        }

        public virtual void ConnectChildrenWithParent()
        {
            foreach (var item in this.Children)
            {
                item.SetParent(this);

                if (item is Group innerGroup)
                {
                    innerGroup.ConnectChildrenWithParent();
                }
            }
        }
    }
}
