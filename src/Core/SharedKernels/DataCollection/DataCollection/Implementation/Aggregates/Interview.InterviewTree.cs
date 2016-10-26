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
        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state = null)
        {
            var sections = this.BuildInterviewTreeSections(questionnaire, state).ToList();

            return new InterviewTree(this.EventSourceId, questionnaire, sections);
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(sectionIdentity, questionnaire, state);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(Identity sectionIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            var section = InterviewTree.CreateSection(questionnaire, sectionIdentity);

            section.AddChildren(this.BuildInterviewTreeGroupChildren(sectionIdentity, questionnaire, interviewState).ToList());

            if (interviewState.IsGroupDisabled(sectionIdentity))
                section.Disable();

            return section;
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            var subSection = InterviewTree.CreateSubSection(questionnaire, groupIdentity);

            subSection.AddChildren(this.BuildInterviewTreeGroupChildren(groupIdentity, questionnaire, state).ToList());

            if (state?.IsGroupDisabled(groupIdentity) ?? false)
                subSection.Disable();

            return subSection;
        }

        private static InterviewTreeVariable BuildInterviewTreeVariable(InterviewStateDependentOnAnswers state,
            Identity childVariableIdentity)
        {
            var variable = InterviewTree.CreateVariable(childVariableIdentity);

            if (state?.IsVariableDisabled(childVariableIdentity) ?? false)
                variable.Disable();

            return variable;
        }

        private static InterviewTreeStaticText BuildInterviewTreeStaticText(InterviewStateDependentOnAnswers state,
            Identity staticTextIdentity)
        {
            var staticText = InterviewTree.CreateStaticText(staticTextIdentity);

            if (state?.IsStaticTextDisabled(staticTextIdentity) ?? false)
                staticText.Disable();

            if (state?.InvalidStaticTexts.ContainsKey(staticTextIdentity) ?? false)
                staticText.MarkAsInvalid(state.InvalidStaticTexts[staticTextIdentity]);
            return staticText;
        }

        private InterviewTreeRoster BuildInterviewTreeRoster(Identity rosterIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            var children = BuildInterviewTreeGroupChildren(rosterIdentity, questionnaire, state).ToList();
            bool isDisabled = state.IsGroupDisabled(rosterIdentity);
            string rosterGroupKey = ConversionHelper.ConvertIdAndRosterVectorToString(rosterIdentity.Id, rosterIdentity.RosterVector);
            string rosterTitle = state?.RosterTitles.ContainsKey(rosterGroupKey) ?? false
                ? state.RosterTitles[rosterGroupKey]
                : null;

            var rosterTitleQuestionId = questionnaire.GetRosterTitleQuestionId(rosterIdentity.Id);
            Identity rosterTitleQuestionIdentity = null;
            if (rosterTitleQuestionId.HasValue)
                rosterTitleQuestionIdentity = new Identity(rosterTitleQuestionId.Value, rosterIdentity.RosterVector);
            RosterType rosterType = RosterType.Fixed;
            Guid? sourceQuestionId = null;
            if (questionnaire.IsFixedRoster(rosterIdentity.Id))
            {
                rosterType = RosterType.Fixed;
            }
            else
            {
                sourceQuestionId = questionnaire.GetRosterSizeQuestion(rosterIdentity.Id);
                var questionaType = questionnaire.GetQuestionType(sourceQuestionId.Value);
                switch (questionaType)
                {
                    case QuestionType.MultyOption:
                        rosterType = questionnaire.IsQuestionYesNo(sourceQuestionId.Value)
                            ? RosterType.YesNo
                            : RosterType.Multi;
                        break;
                    case QuestionType.Numeric:
                        rosterType = RosterType.Numeric;
                        break;
                    case QuestionType.TextList:
                        rosterType = RosterType.List;
                        break;
                }
            }

            var roster = new InterviewTreeRoster(rosterIdentity, children,
                rosterType: rosterType,
                rosterTitle: rosterTitle,
                rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                rosterSizeQuestion: sourceQuestionId,
                childrenReferences: questionnaire.GetChidrenReferences(rosterIdentity.Id));

            if (isDisabled)
                roster.Disable();

            return roster;
        }

        private static InterviewTreeQuestion BuildInterviewTreeQuestion(IQuestionnaire questionnaire,
            InterviewStateDependentOnAnswers state, Identity childQuestionIdentity)
        {
            var question = InterviewTree.CreateQuestion(questionnaire, childQuestionIdentity);

            if (question.IsLinked)
            {
                var optionsForLinkedQuestion = state.GetOptionsForLinkedQuestion(childQuestionIdentity);
                if (optionsForLinkedQuestion != null)
                {
                    question.AsLinked.SetOptions(optionsForLinkedQuestion);
                }
            }

            if (state?.IsQuestionDisabled(childQuestionIdentity) ?? false)
                question.Disable();

            var answer = state?.GetAnswer(childQuestionIdentity);
            if (answer != null)
            {
                question.SetAnswer(answer);
            }

            if (state?.InvalidAnsweredQuestions.ContainsKey(childQuestionIdentity) ?? false)
                question.MarkAsInvalid(state.InvalidAnsweredQuestions[childQuestionIdentity]);

            return question;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(Identity groupIdentity, IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state)
        {
            var childIds = questionnaire.GetChildEntityIds(groupIdentity.Id);

            foreach (var childId in childIds)
            {
                if (questionnaire.IsRosterGroup(childId))
                {
                    Guid[] rostersStartingFromTop = questionnaire.GetRostersFromTopToSpecifiedGroup(childId).ToArray();

                    IEnumerable<RosterVector> childRosterVectors = ExtendRosterVector(
                        state, groupIdentity.RosterVector, rostersStartingFromTop.Length, rostersStartingFromTop);

                    foreach (var childRosterVector in childRosterVectors)
                    {
                        var childRosterIdentity = new Identity(childId, childRosterVector);
                        yield return this.BuildInterviewTreeRoster(childRosterIdentity, questionnaire, state);
                    }
                }
                else if (questionnaire.HasGroup(childId))
                {
                    var childGroupIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return this.BuildInterviewTreeSubSection(childGroupIdentity, questionnaire, state);
                }
                else if (questionnaire.HasQuestion(childId))
                {
                    var childQuestionIdentity = new Identity(childId, groupIdentity.RosterVector);

                    yield return BuildInterviewTreeQuestion(questionnaire, state, childQuestionIdentity);
                }
                else if (questionnaire.IsStaticText(childId))
                {
                    var staticTextIdentity = new Identity(childId, groupIdentity.RosterVector);

                    yield return BuildInterviewTreeStaticText(state, staticTextIdentity);
                }
                else if (questionnaire.IsVariable(childId))
                {
                    var childVariableIdentity = new Identity(childId, groupIdentity.RosterVector);

                    yield return BuildInterviewTreeVariable(state, childVariableIdentity);
                }
            }
        }
    }
}
