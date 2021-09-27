using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public abstract class RosterManager
    {
        protected InterviewTree interviewTree;
        protected readonly IQuestionnaire questionnaire;
        protected Guid rosterId;
        private readonly ISubstitutionTextFactory textFactory;

        protected RosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId,
            ISubstitutionTextFactory textFactory)
        {
            this.interviewTree = interviewTree;
            this.questionnaire = questionnaire;
            this.rosterId = rosterId;
            this.textFactory = textFactory;
        }

        public abstract List<Identity> CalcuateExpectedIdentities(Identity parentIdentity);
        public abstract InterviewTreeRoster CreateRoster(Guid id);

        public virtual void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            roster.SetIdentity(rosterIdentity);
            roster.SortIndex = sortIndex;
            roster.SetTitle(GetGroupTitle(rosterIdentity));
        }

        protected InterviewTreeQuestion GetRosterSizeQuestion(Identity parentIdentity, Guid sizeQuestionId)
        {
            var parentGroup = this.interviewTree.GetGroup(parentIdentity);
            var rosterSizeQuestion = parentGroup.GetQuestionFromThisOrUpperLevel(sizeQuestionId);
            return rosterSizeQuestion;
        }

        protected SubstitutionText GetGroupTitle(Identity rosterIdentity)
        {
            var title = this.questionnaire.GetGroupTitle(this.rosterId);
            return this.textFactory.CreateText(rosterIdentity, title, this.questionnaire);
        }
    } 

    public class FixedRosterManager : RosterManager
    {
        private readonly FixedRosterTitle[] rosterTitles;

        public FixedRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitutionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterTitles = questionnaire.GetFixedRosterTitles(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            return rosterTitles.Select(x => new RosterIdentity(rosterId, parentIdentity.RosterVector, x.Value).ToIdentity()).ToList();
        }

        public override InterviewTreeRoster CreateRoster(Guid id) => new InterviewTreeRoster(
            rosterType: RosterType.Fixed,
            childrenReferences: this.questionnaire.GetChildrenReferences(id));

        public override void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            base.UpdateRoster(roster, parentIdentity, rosterIdentity, sortIndex);
            
            var lastRosterVectorValue = rosterIdentity.RosterVector.Last();

            for (var index = 0; index < this.rosterTitles.Length; index++)
            {
                var rosterTitleInstance = this.rosterTitles[index];
                if (rosterTitleInstance.Value != lastRosterVectorValue) continue;
                
                roster.SetRosterTitle(rosterTitleInstance.Title);
                break;
            }
        }
    }

    public class NumericRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        private Guid? rosterTitleQuestionId;

        public NumericRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitutionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId)?.GetAsInterviewTreeIntegerQuestion();
            var rosterTriggerValue = rosterSizeQuestion?.IsAnswered() ?? false? Math.Max(rosterSizeQuestion.GetAnswer().Value, 0) : 0;
            return Enumerable.Range(0, rosterTriggerValue)
                .Select(index => 
                    new Identity(rosterId, parentIdentity.RosterVector.ExtendWithOneCoordinate(index)))
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Guid id) => new InterviewTreeRoster(
            rosterType: RosterType.Numeric,
            rosterSizeQuestion: this.rosterSizeQuestionId,
            childrenReferences: this.questionnaire.GetChildrenReferences(id));

        public override void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            base.UpdateRoster(roster, parentIdentity, rosterIdentity, sortIndex);

            
            Identity rosterTitleQuestionIdentity = null;
            string rosterTitle;

            if (this.rosterTitleQuestionId.HasValue)
            {
                rosterTitleQuestionIdentity = new Identity(this.rosterTitleQuestionId.Value,
                    rosterIdentity.RosterVector);

                rosterTitle = this.interviewTree.GetQuestion(rosterTitleQuestionIdentity)?.GetAnswerAsString();
            }
            else rosterTitle = (rosterIdentity.RosterVector.Last() + 1).ToString(CultureInfo.InvariantCulture);
            

            roster.AsNumeric.RosterTitleQuestionIdentity = rosterTitleQuestionIdentity;
            roster.SetRosterTitle(rosterTitle);
        }
    }

    public class ListRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        public ListRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitutionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId)?.GetAsInterviewTreeTextListQuestion();
            var listAnswer = rosterSizeQuestion?.IsAnswered() ?? false ? rosterSizeQuestion.GetAnswer().ToTupleArray() : new Tuple<decimal, string>[0];
            return listAnswer
                .Select(answer => new RosterIdentity(rosterId, parentIdentity.RosterVector, answer.Item1, 0).ToIdentity())
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Guid id) => new InterviewTreeRoster(
            rosterType: RosterType.List,
            rosterSizeQuestion: this.rosterSizeQuestionId,
            childrenReferences: this.questionnaire.GetChildrenReferences(id));

        public override void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            base.UpdateRoster(roster, parentIdentity, rosterIdentity, sortIndex);

            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var rosterTitle = (rosterSizeQuestion.GetAsInterviewTreeTextListQuestion()).GetTitleByItemCode(rosterIdentity.RosterVector.Last());
            
            roster.SetRosterTitle(rosterTitle);
        }
    }

    public class MultiRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        private readonly bool shouldQuestionRecordAnswersOrder;
        public MultiRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitutionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            shouldQuestionRecordAnswersOrder = questionnaire.ShouldQuestionRecordAnswersOrder(rosterSizeQuestionId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId)?.GetAsInterviewTreeMultiOptionQuestion();
            decimal[] newMultiAnswer = rosterSizeQuestion?.IsAnswered() ?? false ? rosterSizeQuestion.GetAnswer().ToDecimals().ToArray() : new decimal[0];

            return this.shouldQuestionRecordAnswersOrder
                ? this.GetIdentitiesByUserDefinedOrder(parentIdentity, newMultiAnswer)
                : this.GetIdentitiesByQuestionnaireDefinedOrder(parentIdentity, newMultiAnswer);
        }

        private List<Identity> GetIdentitiesByQuestionnaireDefinedOrder(Identity parentIdentity, decimal[] newMultiAnswer)
        {
            var questionnaireDefinedOrder = new List<Identity>();
            var index = 0;

            var isFilteredCombobox = this.questionnaire.IsQuestionFilteredCombobox(this.rosterSizeQuestionId);
            var optionValues = isFilteredCombobox
                ? this.questionnaire.GetCategoricalMultiOptionsByValues(this.rosterSizeQuestionId, newMultiAnswer.Select(Convert.ToInt32).ToArray()).Select(x => x.Value)
                : this.questionnaire.GetMultiSelectAnswerOptionsAsValues(this.rosterSizeQuestionId);

            foreach (var optionValue in optionValues)
            {
                if (!newMultiAnswer.Contains(optionValue)) continue;
                questionnaireDefinedOrder.Add(
                    new RosterIdentity(this.rosterId, parentIdentity.RosterVector, optionValue, index).ToIdentity());
                index++;
            }

            return questionnaireDefinedOrder;
        }

        private List<Identity> GetIdentitiesByUserDefinedOrder(Identity parentIdentity, decimal[] newMultiAnswer)
            => newMultiAnswer
                .Select((optionValue, i) =>
                    new RosterIdentity(this.rosterId, parentIdentity.RosterVector, optionValue, i).ToIdentity())
                .ToList();


        public override InterviewTreeRoster CreateRoster(Guid id) => new InterviewTreeRoster(
            rosterType: RosterType.Multi,
            rosterSizeQuestion: this.rosterSizeQuestionId,
            childrenReferences: this.questionnaire.GetChildrenReferences(id));

        public override void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            base.UpdateRoster(roster, parentIdentity, rosterIdentity, sortIndex);

            var rosterTitle = questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, rosterIdentity.RosterVector.Last(), null);
            roster.SetRosterTitle(rosterTitle);
        }
    }

    public class YesNoRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        private readonly bool shouldQuestionRecordAnswersOrder;

        public YesNoRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitutionTextFactory textFactory) : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            shouldQuestionRecordAnswersOrder = questionnaire.ShouldQuestionRecordAnswersOrder(rosterSizeQuestionId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId)?.GetAsInterviewTreeYesNoQuestion();
            var newYesNoAnswer = rosterSizeQuestion?.IsAnswered() ?? false ? rosterSizeQuestion.GetAnswer().ToAnsweredYesNoOptions() : new AnsweredYesNoOption[0];

            return this.shouldQuestionRecordAnswersOrder
                ? this.GetIdentitiesByUserDefinedOrder(parentIdentity, newYesNoAnswer)
                : this.GetIdentitiesByQuestionnaireDefinedOrder(parentIdentity, newYesNoAnswer);
        }

        private List<Identity> GetIdentitiesByUserDefinedOrder(Identity parentIdentity, IEnumerable<AnsweredYesNoOption> newYesNoAnswer)
            => newYesNoAnswer
                .Where(x => x.Yes)
                .Select((selectedYesOption, index) =>
                    new RosterIdentity(this.rosterId, parentIdentity.RosterVector, selectedYesOption.OptionValue, index).ToIdentity())
                .ToList();

        private List<Identity> GetIdentitiesByQuestionnaireDefinedOrder(Identity parentIdentity, IEnumerable<AnsweredYesNoOption> newYesNoAnswer)
        {
            var questionnaireDefinedOrder = new List<Identity>();
            var index = 0;
            foreach (var optionValue in this.questionnaire.GetMultiSelectAnswerOptionsAsValues(this.rosterSizeQuestionId))
            {
                if (newYesNoAnswer.Any(x => x.Yes && x.OptionValue == optionValue))
                {
                    questionnaireDefinedOrder.Add(
                        new RosterIdentity(this.rosterId, parentIdentity.RosterVector, optionValue, index).ToIdentity());
                    index++;
                }
            }

            return questionnaireDefinedOrder;
        }

        public override InterviewTreeRoster CreateRoster(Guid id) => new InterviewTreeRoster(
            rosterType: RosterType.YesNo,
            rosterSizeQuestion: this.rosterSizeQuestionId,
            childrenReferences: this.questionnaire.GetChildrenReferences(id));

        public override void UpdateRoster(InterviewTreeRoster roster, Identity parentIdentity, Identity rosterIdentity, int sortIndex)
        {
            base.UpdateRoster(roster, parentIdentity, rosterIdentity, sortIndex);

            var rosterTitle = questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, rosterIdentity.RosterVector.Last(), null);
            roster.SetRosterTitle(rosterTitle);
        }
    }
}
