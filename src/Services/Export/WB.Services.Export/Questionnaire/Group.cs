using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Infrastructure;

namespace WB.Services.Export.Questionnaire
{
    public enum RosterSizeSourceType
    {
        Question = 0,
        FixedTitles = 1
    }

    [DebuggerDisplay("Group {PublicKey}; Variable {VariableName}")]
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

        public bool HasAnyExportableQuestions => Children.Any(x => x is Question || x is Variable);

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

        private QuestionnaireDocument root;
        public QuestionnaireDocument Root
        {
            get
            {
                if (root != null) return root;

                var parent = this.GetParent();
                while (parent != null)
                {
                    if (parent is QuestionnaireDocument document)
                    {
                        this.root = document;
                        return document;
                    }
                    parent = parent.GetParent();
                }

                return null;
            }
        }

        private string tableName;
        public virtual string TableName
        {
            get
            {
                if (tableName != null) return tableName;
                return tableName = Root.DatabaseStructure.GetDataTableName(PublicKey);
            }
        }

        private string enablementTableName, validityTableName;
        public string EnablementTableName => enablementTableName ?? (enablementTableName = Root.DatabaseStructure.GetEnablementDataTableName(PublicKey));
        public string ValidityTableName => validityTableName ?? (validityTableName = Root.DatabaseStructure.GetValidityDataTableName(PublicKey));

        private bool? doesSupportDataTable, doesSupportEnablementTable, doesSupportValidityTable;
        public bool DoesSupportDataTable => doesSupportDataTable ?? (doesSupportDataTable = Root.DatabaseStructure.DoesSupportDataTable(PublicKey)).Value;
        public bool DoesSupportEnablementTable => doesSupportEnablementTable ?? (doesSupportEnablementTable = Root.DatabaseStructure.DoesSupportEnablementTable(PublicKey)).Value;
        public bool DoesSupportValidityTable => doesSupportValidityTable ?? (doesSupportValidityTable = Root.DatabaseStructure.DoesSupportValidityTable(PublicKey)).Value;

        
        private bool? isInRoster;
        public bool IsInsideRoster
        {
            get
            {
                if (isInRoster.HasValue) return isInRoster.Value;
                if (IsRoster) return true;

                var localParent = GetParent();
                while (localParent != null)
                {
                    if (localParent is Group group && group.IsRoster)
                    {
                        this.isInRoster = true;
                    }

                    localParent = localParent.GetParent();
                }

                this.isInRoster = false;
                return this.isInRoster.Value;
            }
        }

        private int? rosterLevel;
        /// <summary>
        /// Count of parent rosters that group has
        /// </summary>
        public int RosterLevel
        {
            get
            {
                int result = this.IsRoster ? 1 : 0;
                var localParent = GetParent();
                while (localParent != null)
                {
                    if (localParent is Group group && group.IsRoster)
                    {
                        result++;
                    }

                    localParent = localParent.GetParent();
                }

                this.rosterLevel = result;
                return this.rosterLevel.Value;
            }
        }

        private string columnName;
        public string ColumnName
        {
            get
            {
                if (columnName != null) return columnName;
                columnName = this.VariableName?.ToLower() ?? PublicKey.ToString().ToLower();
                return columnName;
            }
        }
    }
}
