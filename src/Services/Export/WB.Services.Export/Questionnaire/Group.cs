﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Services.Export.Utils;

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
                tableName = $"{CompressQuestionnaireId(this.Root.QuestionnaireId)}_{this.VariableName ?? this.PublicKey.FormatGuid()}";
                return tableName;
            }
        }
        public string EnablementTableName => $"{TableName}_e";
        public string ValidityTableName => $"{TableName}_v";

        protected string CompressQuestionnaireId(QuestionnaireId questionnaireId)
        {
            var strings = questionnaireId.Id.Split('$');
            strings[0] = Convert.ToBase64String(Guid.Parse(strings[0]).ToByteArray())
                        .Substring(0, 22)
                        .Replace("/", "_")
                        .Replace("+", "-");
            return string.Join("$", strings);
        }
        
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
    }
}
