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
        private readonly ISubstitionTextFactory textFactory;

        protected RosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId,
            ISubstitionTextFactory textFactory)
        {
            this.interviewTree = interviewTree;
            this.questionnaire = questionnaire;
            this.rosterId = rosterId;
            this.textFactory = textFactory;
        }

        public abstract List<Identity> CalcuateExpectedIdentities(Identity parentIdentity);
        public abstract InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index);

        protected InterviewTreeQuestion GetRosterSizeQuestion(Identity parentIdentity, Guid sizeQuestionId)
        {
            var parentGroup = this.interviewTree.GetGroup(parentIdentity);
            var rosterSizeQuestion = parentGroup.GetQuestionFromThisOrUpperLevel(sizeQuestionId);
            return rosterSizeQuestion;
        }

        protected SubstitionText GetGroupTitle(Identity rosterIdentity)
        {
            var title = this.questionnaire.GetGroupTitle(this.rosterId);
            return this.textFactory.CreateText(rosterIdentity, title, this.questionnaire);
        }
    } 

    public class FixedRosterManager : RosterManager
    {
        private readonly FixedRosterTitle[] rosterTitles;

        public FixedRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterTitles = questionnaire.GetFixedRosterTitles(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            return rosterTitles.Select(x => new RosterIdentity(rosterId, parentIdentity.RosterVector, x.Value).ToIdentity()).ToList();
        }

        public override InterviewTreeRoster CreateRoster(Identity identity, Identity rosterIdentity, int index)
        {
            var rosterTitle = this.rosterTitles.Single(x => x.Value == rosterIdentity.RosterVector.Last());
            var title = GetGroupTitle(rosterIdentity);
            return new InterviewTreeRoster(rosterIdentity,
                                     title,
                                     Enumerable.Empty<IInterviewTreeNode>(),
                                     sortIndex: index,
                                     rosterType: RosterType.Fixed,
                                     rosterTitle: rosterTitle.Title,
                                     childrenReferences: this.questionnaire.GetChidrenReferences(rosterIdentity.Id));
        }
    }

    public class NumericRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        private Guid? rosterTitleQuestionId;

        public NumericRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
            rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var integerAnswer = (rosterSizeQuestion != null && rosterSizeQuestion.AsInteger.IsAnswered) ? rosterSizeQuestion.AsInteger.GetAnswer().Value : 0;
            return Enumerable.Range(0, integerAnswer)
                .Select(index => new RosterIdentity(rosterId, parentIdentity.RosterVector, index, index).ToIdentity())
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index)
        {
            var rosterTitle = rosterTitleQuestionId.HasValue ? null : (rosterIdentity.RosterVector.Last() + 1).ToString(CultureInfo.InvariantCulture);
            var rosterTitleQuestionIdentity = this.rosterTitleQuestionId.HasValue 
                ? new Identity(this.rosterTitleQuestionId.Value, rosterIdentity.RosterVector)
                : null;
            var title = GetGroupTitle(rosterIdentity);
            return new InterviewTreeRoster(rosterIdentity,
                                    title,
                                    Enumerable.Empty<IInterviewTreeNode>(),
                                    sortIndex: index,
                                    rosterType: RosterType.Numeric,
                                    rosterTitle: rosterTitle,
                                    rosterSizeQuestion: rosterSizeQuestionId,
                                    rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                                    childrenReferences: this.questionnaire.GetChidrenReferences(rosterIdentity.Id));
        }
    }

    public class ListRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        public ListRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var listAnswer = rosterSizeQuestion.AsTextList.IsAnswered ? rosterSizeQuestion.AsTextList.GetAnswer().ToTupleArray() : new Tuple<decimal, string>[0];
            return listAnswer
                .Select(answer => new RosterIdentity(rosterId, parentIdentity.RosterVector, answer.Item1, 0).ToIdentity())
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var rosterTitle = rosterSizeQuestion.AsTextList.GetTitleByItemCode(rosterIdentity.RosterVector.Last());
            var title = GetGroupTitle(rosterIdentity);

            return new InterviewTreeRoster(rosterIdentity,
                                    title,
                                    Enumerable.Empty<IInterviewTreeNode>(),
                                    sortIndex: index,
                                    rosterType: RosterType.List,
                                    rosterTitle: rosterTitle,
                                    rosterSizeQuestion: rosterSizeQuestionId,
                                    childrenReferences: this.questionnaire.GetChidrenReferences(rosterIdentity.Id));
        }

    }

    public class MultiRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        public MultiRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitionTextFactory textFactory)
            : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var newMultiAnswer = rosterSizeQuestion.AsMultiFixedOption.IsAnswered ? rosterSizeQuestion.AsMultiFixedOption.GetAnswer().ToDecimals() : new decimal[0];

            return newMultiAnswer
                .Select((optionValue, index) => new RosterIdentity(rosterId, parentIdentity.RosterVector, optionValue, index).ToIdentity())
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index)
        {
            var rosterTitle = questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, rosterIdentity.RosterVector.Last());
            var title = GetGroupTitle(rosterIdentity);

            return new InterviewTreeRoster(rosterIdentity,
                                    title,
                                    Enumerable.Empty<IInterviewTreeNode>(),
                                    sortIndex: index,
                                    rosterType: RosterType.Multi,
                                    rosterTitle: rosterTitle,
                                    rosterSizeQuestion: rosterSizeQuestionId,
                                    childrenReferences: this.questionnaire.GetChidrenReferences(rosterIdentity.Id));
        }
    }

    public class YesNoRosterManager : RosterManager
    {
        private readonly Guid rosterSizeQuestionId;
        public YesNoRosterManager(InterviewTree interviewTree, IQuestionnaire questionnaire, Guid rosterId, ISubstitionTextFactory textFactory) : base(interviewTree, questionnaire, rosterId, textFactory)
        {
            rosterSizeQuestionId = questionnaire.GetRosterSizeQuestion(rosterId);
        }

        public override List<Identity> CalcuateExpectedIdentities(Identity parentIdentity)
        {
            var rosterSizeQuestion = this.GetRosterSizeQuestion(parentIdentity, this.rosterSizeQuestionId);
            var newYesNoAnswer = rosterSizeQuestion.AsYesNo.IsAnswered ? rosterSizeQuestion.AsYesNo.GetAnswer().ToAnsweredYesNoOptions() : new AnsweredYesNoOption[0];
            return newYesNoAnswer
                .Where(x => x.Yes)
                .Select((selectedYesOption, index) => new RosterIdentity(rosterId, parentIdentity.RosterVector, selectedYesOption.OptionValue, index).ToIdentity())
                .ToList();
        }

        public override InterviewTreeRoster CreateRoster(Identity parentIdentity, Identity rosterIdentity, int index)
        {
            var rosterTitle = questionnaire.GetAnswerOptionTitle(rosterSizeQuestionId, rosterIdentity.RosterVector.Last());
            var title = GetGroupTitle(rosterIdentity);
            return new InterviewTreeRoster(rosterIdentity,
                                    title,
                                    Enumerable.Empty<IInterviewTreeNode>(),
                                    sortIndex: index,
                                    rosterType: RosterType.YesNo,
                                    rosterTitle: rosterTitle,
                                    rosterSizeQuestion: rosterSizeQuestionId,
                                    childrenReferences: this.questionnaire.GetChidrenReferences(rosterIdentity.Id));
        }
    }
}