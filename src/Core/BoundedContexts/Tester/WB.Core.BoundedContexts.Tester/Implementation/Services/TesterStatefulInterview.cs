using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    /// <summary>
    /// Tester can replay history of an interview against a questionnaire that was changed meanwhile,
    /// so persisted events may reference entities that no longer exist in the tree. Such events are skipped.
    /// </summary>
    public class TesterStatefulInterview : StatefulInterview
    {
        public TesterStatefulInterview(
            ISubstitutionTextFactory substitutionTextFactory,
            IInterviewTreeBuilder treeBuilder,
            IQuestionOptionsRepository optionsRepository)
            : base(substitutionTextFactory, treeBuilder, optionsRepository)
        {
        }

        protected override void Apply(AnswersDeclaredValid @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.MarkValid();
        }

        protected override void Apply(AnswersDeclaredInvalid @event)
        {
            if (@event.FailedValidationConditions.Count > 0)
            {
                foreach (var failedValidationCondition in @event.FailedValidationConditions)
                {
                    var question = this.Tree.GetQuestion(failedValidationCondition.Key);
                    if (failedValidationCondition.Value?.Count > 0)
                        question?.MarkInvalid(failedValidationCondition.Value);
                    else
                        question?.MarkInvalid();
                }
            }
            else
            {
                foreach (var invalidQuestionIdentity in @event.Questions)
                    this.Tree.GetQuestion(invalidQuestionIdentity)?.MarkInvalid();
            }
        }

        protected override void Apply(AnswersDeclaredPlausible @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.MarkPlausible();
        }

        protected override void Apply(AnswersDeclaredImplausible @event)
        {
            var questionsConditions = @event.GetFailedValidationConditionsDictionary();

            foreach (var questionIdentity in questionsConditions.Keys)
                this.Tree.GetQuestion(questionIdentity)?.MarkImplausible(questionsConditions[questionIdentity]);
        }

        protected override void Apply(StaticTextsDeclaredValid @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity)?.MarkValid();
        }

        protected override void Apply(StaticTextsDeclaredInvalid @event)
        {
            var staticTextsConditions = @event.GetFailedValidationConditionsDictionary();

            foreach (var staticTextIdentity in staticTextsConditions.Keys)
                this.Tree.GetStaticText(staticTextIdentity)?.MarkInvalid(staticTextsConditions[staticTextIdentity]);
        }

        protected override void Apply(StaticTextsDeclaredPlausible @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity)?.MarkPlausible();
        }

        protected override void Apply(StaticTextsDeclaredImplausible @event)
        {
            var staticTextsConditions = @event.GetFailedValidationConditionsDictionary();

            foreach (var staticTextIdentity in staticTextsConditions.Keys)
                this.Tree.GetStaticText(staticTextIdentity)?.MarkImplausible(staticTextsConditions[staticTextIdentity]);
        }

        protected override void Apply(GroupsDisabled @event)
        {
            foreach (var groupIdentity in @event.Groups)
                this.Tree.GetGroup(groupIdentity)?.Disable();
        }

        protected override void Apply(VariablesDisabled @event)
        {
            foreach (var variableIdentity in @event.Variables)
                this.Tree.GetVariable(variableIdentity)?.Disable();
        }

        protected override void Apply(QuestionsDisabled @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.Disable();
        }

        protected override void Apply(StaticTextsDisabled @event)
        {
            foreach (var staticTextIdentity in @event.StaticTexts)
                this.Tree.GetStaticText(staticTextIdentity)?.Disable();
        }
    }
}
