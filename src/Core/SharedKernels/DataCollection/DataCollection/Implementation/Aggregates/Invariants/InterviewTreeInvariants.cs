using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewTreeInvariants
    {
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

            var options = question.GetLinkedOptions();

            if (!options.Contains(option))
                throw new InterviewException(
                    $"Answer on linked categorical question {question.FormatForException()} cannot be saved. " +
                    $"Specified option {option} is absent. " +
                    $"Available options: {string.Join(", ", options)}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }

        public void RequireCascadingQuestionAnswerCorrespondsToParentAnswer(Identity cascadingQuestionIdentity, decimal answer,
            IQuestionnaire questionnaire) // TODO: KP-7836 get rid of questionnaire here and use options repository an injected service
        {
            var cascadingQuestion = this.InterviewTree.GetQuestion(cascadingQuestionIdentity);

            if (!cascadingQuestion.IsCascading())
                return;

            int answerParentValue = questionnaire.GetCascadingParentValue(cascadingQuestionIdentity.Id, answer);

            var parentQuestion = cascadingQuestion.GetCascadingParentQuestion();

            if (!parentQuestion.IsAnswered())
                return;

            int actualParentValue = parentQuestion.GetIntegerAnswer();

            if (answerParentValue != actualParentValue)
                throw new AnswerNotAcceptedException(
                    $"For question {cascadingQuestion.FormatForException()} was provided " + 
                    $"selected value {answer} as answer with parent value {answerParentValue}, " +
                    $"but this do not correspond to the parent answer selected value {actualParentValue}. " +
                    $"Interview ID: {this.InterviewTree.InterviewId}.");
        }
    }
}