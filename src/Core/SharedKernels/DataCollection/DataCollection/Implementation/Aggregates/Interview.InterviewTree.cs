using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        protected InterviewTree BuildInterviewTree(IQuestionnaire questionnaire, InterviewStateDependentOnAnswers state = null)
        {
            var tree = new InterviewTree(this.EventSourceId, questionnaire, this.substitionTextFactory, new List<InterviewTreeSection>());
            var sections = this.BuildInterviewTreeSections(tree, questionnaire, this.substitionTextFactory, state).ToList();

            tree.SetSections(sections);
            return tree;
        }

        private IEnumerable<InterviewTreeSection> BuildInterviewTreeSections(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, InterviewStateDependentOnAnswers state)
        {
            var sectionIds = questionnaire.GetAllSections();

            foreach (var sectionId in sectionIds)
            {
                var sectionIdentity = new Identity(sectionId, RosterVector.Empty);
                var section = this.BuildInterviewTreeSection(tree, sectionIdentity, questionnaire, textFactory, state);

                yield return section;
            }
        }

        private InterviewTreeSection BuildInterviewTreeSection(InterviewTree tree, Identity sectionIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, InterviewStateDependentOnAnswers state)
        {
            var section = InterviewTree.CreateSection(tree, questionnaire, textFactory, sectionIdentity);

            section.AddChildren(this.BuildInterviewTreeGroupChildren(tree, sectionIdentity, questionnaire, textFactory, interviewState).ToList());

            if (interviewState.IsGroupDisabled(sectionIdentity))
                section.Disable();

            return section;
        }

        private InterviewTreeSubSection BuildInterviewTreeSubSection(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, InterviewStateDependentOnAnswers state)
        {
            var subSection = InterviewTree.CreateSubSection(tree, questionnaire, textFactory, groupIdentity);

            subSection.AddChildren(this.BuildInterviewTreeGroupChildren(tree, groupIdentity, questionnaire, textFactory, state).ToList());

            if (state?.IsGroupDisabled(groupIdentity) ?? false)
                subSection.Disable();

            return subSection;
        }

        private static InterviewTreeVariable BuildInterviewTreeVariable(InterviewStateDependentOnAnswers state, Identity childVariableIdentity)
        {
            var variable = InterviewTree.CreateVariable(childVariableIdentity);

            if (state?.IsVariableDisabled(childVariableIdentity) ?? false)
                variable.Disable();

            return variable;
        }

        private static InterviewTreeStaticText BuildInterviewTreeStaticText(InterviewTree tree, InterviewStateDependentOnAnswers state, Identity staticTextIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory)
        {
            var staticText = InterviewTree.CreateStaticText(tree, questionnaire, textFactory, staticTextIdentity);

            if (state?.IsStaticTextDisabled(staticTextIdentity) ?? false)
                staticText.Disable();

            if (state?.InvalidStaticTexts.ContainsKey(staticTextIdentity) ?? false)
                staticText.MarkAsInvalid(state.InvalidStaticTexts[staticTextIdentity]);
            return staticText;
        }

        private InterviewTreeRoster BuildInterviewTreeRoster(InterviewTree tree, Identity rosterIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, InterviewStateDependentOnAnswers state)
        {
            var children = BuildInterviewTreeGroupChildren(tree, rosterIdentity, questionnaire, textFactory, state).ToList();
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
            var title = textFactory.CreateText(rosterIdentity, questionnaire.GetGroupTitle(rosterIdentity.Id), questionnaire, tree);

            var roster = new InterviewTreeRoster(rosterIdentity,
                title,
                children,
                rosterType: rosterType,
                rosterTitle: rosterTitle,
                rosterTitleQuestionIdentity: rosterTitleQuestionIdentity,
                rosterSizeQuestion: sourceQuestionId,
                childrenReferences: questionnaire.GetChidrenReferences(rosterIdentity.Id));

            if (isDisabled)
                roster.Disable();

            return roster;
        }

        private static InterviewTreeQuestion BuildInterviewTreeQuestion(InterviewTree tree, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory,
            InterviewStateDependentOnAnswers state, Identity childQuestionIdentity)
        {
            var question = InterviewTree.CreateQuestion(tree, questionnaire, textFactory, childQuestionIdentity);

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
                question.SetObjectAnswer(answer);
            }

            if (state?.InvalidAnsweredQuestions.ContainsKey(childQuestionIdentity) ?? false)
                question.MarkAsInvalid(state.InvalidAnsweredQuestions[childQuestionIdentity]);

            return question;
        }

        private IEnumerable<IInterviewTreeNode> BuildInterviewTreeGroupChildren(InterviewTree tree, Identity groupIdentity, IQuestionnaire questionnaire, ISubstitionTextFactory textFactory, InterviewStateDependentOnAnswers state)
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
                        yield return this.BuildInterviewTreeRoster(tree, childRosterIdentity, questionnaire, textFactory, state);
                    }
                }
                else if (questionnaire.HasGroup(childId))
                {
                    var childGroupIdentity = new Identity(childId, groupIdentity.RosterVector);
                    yield return this.BuildInterviewTreeSubSection(tree, childGroupIdentity, questionnaire, textFactory, state);
                }
                else if (questionnaire.HasQuestion(childId))
                {
                    var childQuestionIdentity = new Identity(childId, groupIdentity.RosterVector);

                    yield return BuildInterviewTreeQuestion(tree, questionnaire, textFactory, state, childQuestionIdentity);
                }
                else if (questionnaire.IsStaticText(childId))
                {
                    var staticTextIdentity = new Identity(childId, groupIdentity.RosterVector);

                    yield return BuildInterviewTreeStaticText(tree, state, staticTextIdentity, questionnaire, textFactory);
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
