using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.InterviewDataStorage.InterviewDataExport;

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
        public Group(List<IQuestionnaireEntity>? children = null)
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

        public IQuestionnaireEntity? Parent { get; private set; }

        public FixedRosterTitle[] FixedRosterTitles { get; set; } = Array.Empty<FixedRosterTitle>();
        
        public string VariableName { get; set; } = String.Empty;

        public string Title { get; set; }  = String.Empty;

        public Guid? RosterTitleQuestionId { get; set;  }

        public Guid PublicKey { get; set;  }

        public IEnumerable<IQuestionnaireEntity> Children { get; set; } 

        public bool HasAnyExportableChild => Children.Any(x => x.IsExportable);

        public IQuestionnaireEntity? GetParent()
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

        private QuestionnaireDocument? root;
        public QuestionnaireDocument Root
        {
            get
            {
                if (root != null) return root;

                if (this is QuestionnaireDocument doc)
                {
                    this.root = doc;
                    return this.root;
                }

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

                throw new InvalidOperationException("Questionnaire Document was not found.");
            }
        }

        private string? tableName;
        public virtual string TableName
        {
            get
            {
                if (tableName != null) return tableName;
                return tableName = Root.DatabaseStructure.GetDataTableName(PublicKey);
            }
        }

        private string? enablementTableName, validityTableName;
        public string EnablementTableName => enablementTableName ??= Root.DatabaseStructure.GetEnablementDataTableName(PublicKey);
        public string ValidityTableName => validityTableName ??= Root.DatabaseStructure.GetValidityDataTableName(PublicKey);

        private bool? doesSupportDataTable, doesSupportEnablementTable, doesSupportValidityTable;

        public bool DoesSupportDataTable => doesSupportDataTable ?? (doesSupportDataTable = this.IsRoster || Children.Any(c => c is Question || c is Variable)).Value;
        public bool DoesSupportEnablementTable => doesSupportEnablementTable ?? (doesSupportEnablementTable = IsRoster || Children.Any(c => c is Question || c is Variable || c is StaticText)).Value;
        public bool DoesSupportValidityTable => doesSupportValidityTable ?? (doesSupportValidityTable = Children.Any(c => c is Question || c is StaticText)).Value;
        
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
                        return this.isInRoster.Value;
                    }

                    localParent = localParent.GetParent();
                }

                this.isInRoster = false;
                return this.isInRoster.Value;
            }
        }

        private string? columnName;
        public string ColumnName
        {
            get
            {
                if (columnName != null) return columnName;
                columnName = !string.IsNullOrWhiteSpace(this.VariableName)
                                ? PostgresSystemColumns.Escape(this.VariableName.ToLower())
                                : PublicKey.ToString().ToLower();
                if (string.IsNullOrEmpty(columnName))
                    throw new ArgumentException($"Column name cant be empty. Entity: {PublicKey}, Questionnaire: {Root.QuestionnaireId.Id}");
                return columnName;
            }
        }
    }
}
