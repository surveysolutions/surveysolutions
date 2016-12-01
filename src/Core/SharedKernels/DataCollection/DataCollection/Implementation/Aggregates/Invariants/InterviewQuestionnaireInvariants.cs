using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewQuestionnaireInvariants
    {
        public string InterviewId { get; }
        private IQuestionnaire Questionnaire { get; }

        public InterviewQuestionnaireInvariants(string interviewId, IQuestionnaire questionnaire)
        {
            this.InterviewId = interviewId;
            this.Questionnaire = questionnaire;
        }

        public void RequireQuestionExists(Guid questionId)
        {
            if (!this.Questionnaire.HasQuestion(questionId))
                throw new InterviewException(
                    $"Question is missing. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewId}.");
        }

        public void RequireQuestionType(Guid questionId, params QuestionType[] expectedQuestionTypes)
        {
            QuestionType actualQuestionType = this.Questionnaire.GetQuestionType(questionId);

            if (!expectedQuestionTypes.Contains(actualQuestionType))
                throw new AnswerNotAcceptedException(
                    $"Question has type {actualQuestionType}. " +
                    $"But one of the following types was expected: {string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))}. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewId}.");
        }

        public void RequireNumericRealQuestion(Guid questionId)
        {
            if (this.Questionnaire.IsQuestionInteger(questionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type real. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewId}.");
        }

        public void RequireNumericIntegerQuestion(Guid questionId)
        {
            if (!this.Questionnaire.IsQuestionInteger(questionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type integer. " +
                    $"Question ID: {questionId.FormatGuid()}. " +
                    $"Interview ID: {this.InterviewId}.");
        }
    }
}