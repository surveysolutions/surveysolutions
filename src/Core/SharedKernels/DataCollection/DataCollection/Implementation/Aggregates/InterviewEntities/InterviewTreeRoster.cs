using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    [DebuggerDisplay("{ToString()}")]
    public class InterviewTreeRoster : InterviewTreeGroup
    {
        public InterviewTreeRoster(Identity identity,
            SubstitionText title,
            IEnumerable<IInterviewTreeNode> children,
            string rosterTitle = null,
            int sortIndex = 0,
            RosterType rosterType = RosterType.Fixed,
            Guid? rosterSizeQuestion = null,
            Identity rosterTitleQuestionIdentity = null,
            IEnumerable<QuestionnaireItemReference> childrenReferences = null)
            : base(identity, title, childrenReferences)
        {
            this.RosterTitle = rosterTitle;
            this.SortIndex = sortIndex;
            this.AddChildren(children);
            switch (rosterType)
            {
                case RosterType.Fixed:
                    this.AsFixed = new InterviewTreeFixedRoster();
                    break;
                case RosterType.Numeric:
                    this.AsNumeric = new InterviewTreeNumericRoster(rosterSizeQuestion.Value,
                        rosterTitleQuestionIdentity);
                    break;
                case RosterType.YesNo:
                    this.AsYesNo = new InterviewTreeYesNoRoster(rosterSizeQuestion.Value);
                    break;
                case RosterType.Multi:
                    this.AsMulti = new InterviewTreeMultiRoster(rosterSizeQuestion.Value);
                    break;
                case RosterType.List:
                    this.AsList = new InterviewTreeListRoster(rosterSizeQuestion.Value);
                    break;
            }
        }

        public string RosterTitle { get; set; }
        public int SortIndex { get; set; } = 0;

        public InterviewTreeNumericRoster AsNumeric { get; private set; }
        public InterviewTreeListRoster AsList { get; private set; }
        public InterviewTreeYesNoRoster AsYesNo { get; private set; }
        public InterviewTreeMultiRoster AsMulti { get; private set; }
        public InterviewTreeFixedRoster AsFixed { get; private set; }

        public bool IsNumeric => this.AsNumeric != null;
        public bool IsList => this.AsList != null;
        public bool IsYesNo => this.AsYesNo != null;
        public bool IsMulti => this.AsMulti != null;
        public bool IsFixed => this.AsFixed != null;

        public Guid RosterSizeId
        {
            get
            {
                if (this.IsNumeric) return AsNumeric.RosterSizeQuestion;
                if (this.IsMulti) return AsMulti.RosterSizeQuestion;
                if (this.IsYesNo) return AsYesNo.RosterSizeQuestion;
                if (this.IsList) return AsList.RosterSizeQuestion;

                return Identity.Id;
            }
        }


    private string GetTypeAsText()
        {
            if (this.IsNumeric) return "Numeric";
            if (this.IsFixed) return "Fixed";
            if (this.IsMulti) return "Multi";
            if (this.IsYesNo) return "YesNo";
            if (this.IsList) return "List";

            return "no type";
        }
        public override string ToString()
           => $"{this.GetTypeAsText()} Roster ({this.Identity}) [{this.RosterTitle}]" + Environment.NewLine
              + string.Join(Environment.NewLine, this.Children.Select(child => StringExtensions.PrefixEachLine(child.ToString(), "  ")));

        //public override string ToString()
        //    => $"Roster {this.Identity} '{this.Title} - {this.RosterTitle ?? "[...]"}'. " +
        //       $" {(this.IsDisabled() ? "Disabled" : "Enabled")}. ";

        public void SetRosterTitle(string rosterTitle)
        {
            this.RosterTitle = rosterTitle;
        }

        public void UpdateRosterTitle(Func<Guid, decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (IsFixed) return;

            if (this.IsList)
            {
                var sizeQuestionId = this.AsList.RosterSizeQuestion;
                var rosterSizeQuestion = this.GetQuestionFromThisOrUpperLevel(sizeQuestionId);
                this.SetRosterTitle(rosterSizeQuestion.AsTextList.GetTitleByItemCode(this.Identity.RosterVector.Last()));
            }

            else if (this.IsMulti)
            {
                this.SetRosterTitle(getCategoricalAnswerOptionText?.Invoke(this.AsMulti.RosterSizeQuestion, this.Identity.RosterVector.Last()));
            }

            else if (this.IsYesNo)
            {
                this.SetRosterTitle(getCategoricalAnswerOptionText?.Invoke(this.AsYesNo.RosterSizeQuestion, this.Identity.RosterVector.Last()));
            }

            else if (this.IsNumeric)
            {
                var titleQuestion = this.Tree.GetQuestion(this.AsNumeric.RosterTitleQuestionIdentity);
                if (titleQuestion == null) return;
                var rosterTitle = titleQuestion.IsAnswered()
                    ? titleQuestion.GetAnswerAsString()
                    : null;
                this.SetRosterTitle(rosterTitle);
            }
        }

        public override IInterviewTreeNode Clone()
        {
            var clonedInterviewTreeRoster = (InterviewTreeRoster)base.Clone();
            if (this.IsYesNo) clonedInterviewTreeRoster.AsYesNo = this.AsYesNo.Clone();
            if (this.IsFixed) clonedInterviewTreeRoster.AsFixed = this.AsFixed.Clone();
            if (this.IsList) clonedInterviewTreeRoster.AsList = this.AsList.Clone();
            if (this.IsMulti) clonedInterviewTreeRoster.AsMulti = this.AsMulti.Clone();
            if (this.IsNumeric) clonedInterviewTreeRoster.AsNumeric = this.AsNumeric.Clone();

            return clonedInterviewTreeRoster;
        }
    }


    public enum RosterType
    {
        Fixed = 1,
        Numeric = 2,
        YesNo = 3,
        Multi = 4,
        List = 5
    }

    public class InterviewTreeFixedRoster
    {
        public InterviewTreeFixedRoster Clone() => (InterviewTreeFixedRoster)this.MemberwiseClone();
    }

    public class InterviewTreeMultiRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeMultiRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
        public InterviewTreeMultiRoster Clone() => (InterviewTreeMultiRoster)this.MemberwiseClone();
    }

    public class InterviewTreeYesNoRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeYesNoRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }

        public InterviewTreeYesNoRoster Clone() => (InterviewTreeYesNoRoster)this.MemberwiseClone();
    }

    public class InterviewTreeNumericRoster
    {
        public Guid RosterSizeQuestion { get; set; }
        public Identity RosterTitleQuestionIdentity { get; set; }
        public bool HasTitleQuestion => RosterTitleQuestionIdentity != null;

        public InterviewTreeNumericRoster(Guid rosterSizeQuestion, Identity rosterTitleQuestionIdentity)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
            this.RosterTitleQuestionIdentity = rosterTitleQuestionIdentity;
        }
        public InterviewTreeNumericRoster Clone() => (InterviewTreeNumericRoster)this.MemberwiseClone();
    }

    public class InterviewTreeListRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeListRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }

        public InterviewTreeListRoster Clone() => (InterviewTreeListRoster)this.MemberwiseClone();
    }
}