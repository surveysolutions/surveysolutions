using System;
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
    }
}