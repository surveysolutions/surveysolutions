using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    /// <summary>
    /// TesterBoundedContextModule registers this subclass in place of the shared stateful aggregate.
    /// During replay against a changed questionnaire, events referencing missing tree entities are skipped.
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

        protected override void HandleEvent(object evnt)
        {
            if (evnt is QuestionAnswered questionAnswered
                && !HasQuestion(questionAnswered.QuestionId, questionAnswered.RosterVector))
                return;

            base.HandleEvent(evnt);
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

        protected override void Apply(AnswersRemoved @event)
        {
            foreach (var identity in @event.Questions)
            {
                var question = this.Tree.GetQuestion(identity);
                if (question == null)
                    continue;

                question.RemoveAnswer();
                this.ActualizeRostersIfQuestionIsRosterSize(identity.Id);
            }
        }

        protected override void Apply(AnswerRemoved @event)
        {
            var question = this.Tree.GetQuestion(Identity.Create(@event.QuestionId, @event.RosterVector));
            if (question == null)
                return;

            question.RemoveAnswer();
            this.ActualizeRostersIfQuestionIsRosterSize(@event.QuestionId);
        }

        protected override void Apply(AnswersMarkedAsProtected @event)
        {
            foreach (var questionIdentity in @event.Questions)
                this.Tree.GetQuestion(questionIdentity)?.ProtectAnswer();
        }

        protected override void Apply(AnswerCommented @event)
        {
            if (HasQuestion(@event.QuestionId, @event.RosterVector))
                base.Apply(@event);
        }

        protected override void Apply(AnswerCommentResolved @event)
        {
            if (HasQuestion(@event.QuestionId, @event.RosterVector))
                base.Apply(@event);
        }

        public override void Apply(LinkedToListOptionsChanged @event)
        {
            foreach (var linkedQuestion in @event.ChangedLinkedQuestions)
                this.Tree.GetQuestion(linkedQuestion.QuestionId)?.AsLinkedToList.SetOptions(
                    linkedQuestion.Options?.Select(Convert.ToInt32) ?? Array.Empty<int>());
        }

        private bool HasQuestion(Guid questionId, decimal[] rosterVector) =>
            this.Tree.GetQuestion(Identity.Create(questionId, rosterVector)) != null;
    }
}
