using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState = null)
        {
            var sections = this.BuildInterviewTreeSections(questionnaire, interviewState).ToList();

            return new InterviewTree(this.EventSourceId, questionnaire, sections);
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(sectionIdentity, questionnaire, interviewState);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(Identity sectionIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            interviewState = interviewState ?? this.interviewState;

            var children = BuildInterviewTreeGroupChildren(sectionIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(sectionIdentity);

            return new InterviewTreeSection(sectionIdentity, children, isDisabled: isDisabled);
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var children = BuildInterviewTreeGroupChildren(groupIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(groupIdentity);

            return new InterviewTreeSubSection(groupIdentity, children, isDisabled: isDisabled);
        }

        private InterviewTreeRoster BuildInterviewTreeRoster(Identity rosterIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var children = BuildInterviewTreeGroupChildren(rosterIdentity, questionnaire, interviewState).ToList();
            bool isDisabled = interviewState.IsGroupDisabled(rosterIdentity);
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterIdentity.Id, rosterIdentity.RosterVector);
            string rosterTitle = this.interviewState.RosterTitles.ContainsKey(rosterGroupKey)
                ? this.interviewState.RosterTitles[rosterGroupKey]
                : null;

            var rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterIdentity.Id);
            Identity rosterTitleQuestionIdentity = null;
            if (rosterTitleQuestionId.HasValue)
                rosterTitleQuestionIdentity = new Identity(rosterTitleQuestionId.Value, rosterIdentity.RosterVector);
            RosterType rosterType = RosterType.Fixed;
            Guid? sourceQuestionId = null;
            if (questionnaire.IsFixedRoster(rosterIdentity.Id))
                rosterType = RosterType.Fixed;
            else
            {
                sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterIdentity.Id);
                var questionaType = questionnaire.GetQuestionType(sourceQuestionId.Value);
                switch (questionaType)
                {
                    case QuestionType.MultyOption:
                        rosterType = questionnaire.IsQuestionYesNo(sourceQuestionId.Value) ? RosterType.YesNo : RosterType.Multi;
                        break;
                    case QuestionType.Numeric:
                        rosterType = RosterType.Numeric;
                        break;
                    case QuestionType.TextList:
                        rosterType = RosterType.List;
                        break;
                }
            }

            return new InterviewTreeRoster(rosterIdentity, children,
                rosterType: rosterType,
                isDisabled: isDisabled,
                rosterTitle: rosterTitle,
                rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                rosterSizeQuestion: sourceQuestionId);
        }

        private static InterviewTreeQuestion BuildInterviewTreeQuestion(Identity questionIdentity, object answer, bool isQuestionDisabled, IReadOnlyCollection<RosterVector> linkedOptions, IQuestionnaire questionnaire)
        {
            QuestionType questionType = questionnaire.GetQuestionType(questionIdentity.Id);
            bool isDisabled = isQuestionDisabled;
            string title = questionnaire.GetQuestionTitle(questionIdentity.Id);
            string variableName = questionnaire.GetQuestionVariableName(questionIdentity.Id);
            bool isYesNoQuestion = questionnaire.IsQuestionYesNo(questionIdentity.Id);
            bool isDecimalQuestion = !questionnaire.IsQuestionInteger(questionIdentity.Id);
            bool isLinkedQuestion = questionnaire.IsQuestionLinked(questionIdentity.Id) || questionnaire.IsQuestionLinkedToRoster(questionIdentity.Id);
            var linkedSourceEntityId = isLinkedQuestion ?
                (questionnaire.IsQuestionLinked(questionIdentity.Id)
                  ? questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id)
                  : questionnaire.GetRosterReferencedByLinkedQuestion(questionIdentity.Id))
                  : (Guid?)null;

            Guid? commonParentRosterIdForLinkedQuestion = isLinkedQuestion ? questionnaire.GetCommontParentForLinkedQuestionAndItSource(questionIdentity.Id) : null;
            Identity commonParentIdentity = null;
            if (isLinkedQuestion && commonParentRosterIdForLinkedQuestion.HasValue)
            {
                var level = questionnaire.GetRosterLevelForEntity(commonParentRosterIdForLinkedQuestion.Value);
                var commonParentRosterVector = questionIdentity.RosterVector.Take(level).ToArray();
                commonParentIdentity = new Identity(commonParentRosterIdForLinkedQuestion.Value, commonParentRosterVector);
            }

            Guid? cascadingParentQuestionId = questionnaire.GetCascadingQuestionParentId(questionIdentity.Id);
            var cascadingParentQuestionIdentity = cascadingParentQuestionId.HasValue
                ? GetInstanceOfQuestionWithSameAndUpperRosterLevelOrThrow(cascadingParentQuestionId.Value, questionIdentity.RosterVector, questionnaire)
                : null;

            return new InterviewTreeQuestion(
                questionIdentity,
                questionType: questionType,
                isDisabled: isDisabled,
                title: title,
                variableName: variableName,
                answer: answer,
                linkedOptions: linkedOptions,
                cascadingParentQuestionIdentity: cascadingParentQuestionIdentity,
                isYesNo: isYesNoQuestion,
                isDecimal: isDecimalQuestion,
                linkedSourceId: linkedSourceEntityId,
                commonParentRosterIdForLinkedQuestion: commonParentIdentity);
        }

        private static InterviewTreeStaticText BuildInterviewTreeStaticText(Identity staticTextIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            bool isDisabled = interviewState.IsStaticTextDisabled(staticTextIdentity);

            var interviewStaticText = new InterviewTreeStaticText(staticTextIdentity, isDisabled);
            if (interviewState.InvalidStaticTexts.ContainsKey(staticTextIdentity))
                interviewStaticText.MarkAsInvalid(interviewState.InvalidStaticTexts[staticTextIdentity]);
            return interviewStaticText;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers interviewState)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                if (questionnaire.IsRosterGroup(childId))
                {
                    Guid[] rostersStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(childId).ToArray();

                    IEnumerable<RosterVector> childRosterVectors = ExtendRosterVector(
                        interviewState, groupIdentity.RosterVector, rostersStartingFromTop.Length, rostersStartingFromTop);

                    foreach (var childRosterVector in childRosterVectors)
                    {
                        var childRosterIdentity = new Identity(childId, childRosterVector);
                        yield return BuildInterviewTreeRoster(childRosterIdentity, questionnaire, interviewState);
                    }
                }
                else if (questionnaire.HasGroup(childId))
                {
                    var childGroupIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return BuildInterviewTreeSubSection(childGroupIdentity, questionnaire, interviewState);
                }
                else if (questionnaire.HasQuestion(childId))
                {
                    var childQuestionIdentity = new Identity(childId, groupIdentity.RosterVector);
                    var answer = interviewState.GetAnswer(childQuestionIdentity);

                    var interviewTreeQuestion = BuildInterviewTreeQuestion(childQuestionIdentity, answer,
                        interviewState.IsQuestionDisabled(childQuestionIdentity),
                        interviewState.GetOptionsForLinkedQuestion(childQuestionIdentity), questionnaire);

                    if (interviewState.InvalidAnsweredQuestions.ContainsKey(childQuestionIdentity))
                        interviewTreeQuestion.MarkAsInvalid(interviewState.InvalidAnsweredQuestions[childQuestionIdentity]);

                    yield return interviewTreeQuestion;
                }
                else if (questionnaire.IsStaticText(childId))
                {
                    var childStaticTextIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return BuildInterviewTreeStaticText(childStaticTextIdentity, questionnaire, interviewState);
                }
                else if (questionnaire.IsVariable(childId))
                {
                    var childVariableIdentity = new Identity(childId, groupIdentity.RosterVector);
                    bool isDisabled = interviewState.IsVariableDisabled(childVariableIdentity);
                    yield return new InterviewTreeVariable(childVariableIdentity, isDisabled, null);
                }
            }
        }

    }
}
