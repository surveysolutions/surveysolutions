using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewQuestionInvariants
    {
        public InterviewQuestionInvariants(string interviewId, Guid questionId, IQuestionnaire questionnaire)
        {
            this.InterviewId = interviewId;
            this.QuestionId = questionId;
            this.Questionnaire = questionnaire;
        }

        private string InterviewId { get; }
        private Guid QuestionId { get; }
        private IQuestionnaire Questionnaire { get; }

        private string InfoForException => $"Question ID: {this.QuestionId.FormatGuid()}. Interview ID: {this.InterviewId}.";

        public void RequireQuestionExists()
        {
            if (!this.Questionnaire.HasQuestion(this.QuestionId))
                throw new InterviewException(
                    $"Question is missing. {this.InfoForException}");
        }

        public void RequireQuestionType(params QuestionType[] expectedQuestionTypes)
        {
            QuestionType actualQuestionType = this.Questionnaire.GetQuestionType(this.QuestionId);

            if (!expectedQuestionTypes.Contains(actualQuestionType))
                throw new AnswerNotAcceptedException(
                    $"Question has type {actualQuestionType}. " +
                    $"But one of the following types was expected: {string.Join(", ", expectedQuestionTypes.Select(type => type.ToString()))}. " +
                    this.InfoForException);
        }

        public void RequireNumericRealQuestion()
        {
            if (this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type real. {this.InfoForException}");
        }

        public void RequireNumericIntegerQuestion()
        {
            if (!this.Questionnaire.IsQuestionInteger(this.QuestionId))
                throw new AnswerNotAcceptedException(
                    $"Question doesn't support answer of type integer. {this.InfoForException}");
        }
    }
}