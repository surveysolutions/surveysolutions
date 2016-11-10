using System;
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
        private IQuestionOptionsRepository questionOptionsRepository => ServiceLocator.Current.GetInstance<IQuestionOptionsRepository>();

        public InterviewTreeInvariants(InterviewTree interviewTree)
        {
            this.InterviewTree = interviewTree;
        }

        public InterviewTree InterviewTree { get; }

        public void RequireRosterVectorQuestionInstanceExists(Guid questionId, RosterVector rosterVector)
        {
            if (rosterVector == null)
                throw new InterviewException(
                    $"Roster information for question is missing. " +
                    $"Roster vector cannot be null. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");

            var questions = this.InterviewTree.FindQuestions(questionId);
            var rosterVectors = questions.Select(question => question.Identity.RosterVector).ToList();

            if (!rosterVectors.Contains(rosterVector))
                throw new InterviewException(
                    $"Roster information for question is incorrect. " +
                    $"No questions found for roster vector {rosterVector}. " +
                    $"Available roster vectors: {string.Join(", ", rosterVectors)}. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireQuestionIsEnabled(Identity questionIdentity)
        {
            var question = this.InterviewTree.GetQuestion(questionIdentity);

            if (question.IsDisabled())
                throw new InterviewException(
                    $"Question {question.FormatForException()} (or it's parent) is disabled " +
                    $"and question's answer cannot be changed. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireLinkedOptionIsAvailable(Identity linkedQuestionIdentity, RosterVector option)
        {
            var question = this.InterviewTree.GetQuestion(linkedQuestionIdentity);

            if (!question.IsLinked)
                return;

            if (!question.AsLinked.Options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked categorical question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", question.AsLinked.Options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireCascadingQuestionAnswerCorrespondsToParentAnswer(Identity cascadingQuestionIdentity, decimal answer,
            QuestionnaireIdentity questionnaireId, Translation translation) 
        {
            var question = this.InterviewTree.GetQuestion(cascadingQuestionIdentity);

            if (!question.IsCascading)
                return;

            var answerOption = questionOptionsRepository.GetOptionForQuestionByOptionValue(questionnaireId,
                cascadingQuestionIdentity.Id, answer, translation);

            if (!answerOption.ParentValue.HasValue)
                throw new QuestionnaireException(
                    $"Answer option has no parent value. Option value: {answer}, Question id: '{cascadingQuestionIdentity.Id}'.");

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