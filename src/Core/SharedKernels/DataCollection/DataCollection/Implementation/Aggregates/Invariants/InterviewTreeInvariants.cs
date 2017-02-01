using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewTreeInvariants
    {
        private IQuestionOptionsRepository QuestionOptionsRepository => ServiceLocator.Current.GetInstance<IQuestionOptionsRepository>();

        public InterviewTreeInvariants(Identity questionIdentity, InterviewTree interviewTree)
        {
            this.QuestionIdentity = questionIdentity;
            this.InterviewTree = interviewTree;
        }

        public Identity QuestionIdentity { get; }
        private InterviewTree InterviewTree { get; }

        public InterviewTreeInvariants RequireQuestionInstanceExists()
        {
            if (this.QuestionIdentity.RosterVector == null)
                throw new InterviewException(
                    $"Roster information for question is missing. " +
                    $"Roster vector cannot be null. " +
                    $"Question ID: {this.QuestionIdentity.Id.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            var questions = this.InterviewTree.FindQuestions(this.QuestionIdentity.Id);
            var rosterVectors = questions.Select(question => question.Identity.RosterVector).ToList();

            if (!rosterVectors.Contains(this.QuestionIdentity.RosterVector))
                throw new InterviewException(
                    $"Roster information for question is incorrect. " +
                    $"No questions found for roster vector {this.QuestionIdentity.RosterVector}. " +
                    $"Available roster vectors: {string.Join(", ", rosterVectors)}. " +
                    $"Question ID: {this.QuestionIdentity.Id.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            return this;
        }

        public void RequireQuestionIsEnabled()
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (question.IsDisabled())
                throw new InterviewException(
                    $"Question {question.FormatForException()} (or it's parent) is disabled " +
                    $"and question's answer cannot be changed. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireLinkedOptionIsAvailable(RosterVector option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.IsLinked)
                return;

            if (!question.AsLinked.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked categorical question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", question.AsLinked.Options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireLinkedToListOptionIsAvailable(decimal option)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.IsLinkedToListQuestion)
                return;

            if (!question.AsLinkedToList.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked to list question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", question.AsLinked.Options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireCascadingQuestionAnswerCorrespondsToParentAnswer(decimal answer, QuestionnaireIdentity questionnaireId, Translation translation)
        {
            var question = this.InterviewTree.GetQuestion(this.QuestionIdentity);

            if (!question.IsCascading)
                return;

            var answerOption = this.QuestionOptionsRepository.GetOptionForQuestionByOptionValue(questionnaireId,
                this.QuestionIdentity.Id, answer, translation);

            if (!answerOption.ParentValue.HasValue)
                throw new QuestionnaireException(
                    $"Answer option has no parent value. Option value: {answer}, Question id: '{this.QuestionIdentity.Id}'.");

            int answerParentValue = answerOption.ParentValue.Value;
            var parentQuestion = question.AsCascading.GetCascadingParentQuestion();

            if (!parentQuestion.IsAnswered)
                return;

            int actualParentValue = parentQuestion.GetAnswer().SelectedValue;

            if (answerParentValue != actualParentValue)
                throw new AnswerNotAcceptedException(
                    $"For question {question.FormatForException()} was provided " + 
                    $"selected value {answer} as answer with parent value {answerParentValue}, " +
                    $"but this do not correspond to the parent answer selected value {actualParentValue}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }
    }
}