using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewTreeRoster : InterviewTreeGroup
    {
        public InterviewTreeRoster(Identity identity,
            IEnumerable<IInterviewTreeNode> children,
            string rosterTitle = null,
            int sortIndex = 0,
            RosterType rosterType = RosterType.Fixed,
            Guid? rosterSizeQuestion = null,
            Identity rosterTitleQuestionIdentity = null,
            IEnumerable<QuestionnaireItemReference> childrenReferences = null)
            : base(identity, childrenReferences)
        {
            this.RosterTitle = rosterTitle;
            this.SortIndex = sortIndex;
            this.AddChild(children);
            switch (rosterType)
            {
                case RosterType.Fixed:
                    this.AsFixed = new InterviewTreeFixedRoster();
                    break;
                case RosterType.Numeric:
                    this.AsNumeric = new InterviewTreeNumericRoster(rosterSizeQuestion.Value, rosterTitleQuestionIdentity);
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

        public InterviewTreeNumericRoster AsNumeric { get; }
        public InterviewTreeListRoster AsList { get; }
        public InterviewTreeYesNoRoster AsYesNo { get; }
        public InterviewTreeMultiRoster AsMulti { get; }
        public InterviewTreeFixedRoster AsFixed { get; }

        public bool IsNumeric => this.AsNumeric != null;
        public bool IsList => this.AsList != null;
        public bool IsYesNo => this.AsYesNo != null;
        public bool IsMulti => this.AsMulti != null;
        public bool IsFixed => this.AsFixed != null;

        public override string ToString()
            => $"Roster ({this.Identity}) [{this.RosterTitle}]" + Environment.NewLine
               + string.Join(Environment.NewLine, this.Children.Select(child => StringExtensions.PrefixEachLine(child.ToString(), "  ")));

        public void SetRosterTitle(string rosterTitle)
        {
            this.RosterTitle = rosterTitle;
        }

        public void UpdateRosterTitle(Func<Guid, decimal, string> getCategoricalAnswerOptionText = null)
        {
            if (!this.IsList && !this.IsNumeric && !this.IsMulti) return;

            if (this.IsList)
            {
                var sizeQuestionId = this.AsList.RosterSizeQuestion;
                var rosterSizeQuestion = this.GetQuestionFromThisOrUpperLevel(sizeQuestionId);
                this.SetRosterTitle(rosterSizeQuestion.AsTextList.GetTitleByItemCode(this.Identity.RosterVector.Last()));
            }

            if (this.IsMulti)
            {
                this.SetRosterTitle(getCategoricalAnswerOptionText?.Invoke(this.AsMulti.RosterSizeQuestion, this.Identity.RosterVector.Last()));
            }

            if (this.IsNumeric)
            {
                var titleQuestion = this.Tree.GetQuestion(this.AsNumeric.RosterTitleQuestionIdentity);
                if (titleQuestion == null) return;
                var rosterTitle = titleQuestion.IsAnswered()
                    ? titleQuestion.GetAnswerAsString((answerOptionValue) => getCategoricalAnswerOptionText?.Invoke(titleQuestion.Identity.Id, answerOptionValue))
                    : null;
                this.SetRosterTitle(rosterTitle);
            }
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
    }

    public class InterviewTreeMultiRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeMultiRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
    }

    public class InterviewTreeYesNoRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeYesNoRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
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
    }

    public class InterviewTreeListRoster
    {
        public Guid RosterSizeQuestion { get; set; }

        public InterviewTreeListRoster(Guid rosterSizeQuestion)
        {
            this.RosterSizeQuestion = rosterSizeQuestion;
        }
    }
}