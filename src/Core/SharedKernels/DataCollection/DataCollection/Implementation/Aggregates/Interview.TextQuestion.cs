using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private InterviewChanges CalculateInterviewChangesOnAnswerTextQuestion(ILatestInterviewExpressionState expressionProcessorState, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateTextAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, answer, AnswerChangeType.Text,
                answerTime, questionnaire, expressionProcessorState);
        }

        public void AnswerTextQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, string answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);
            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            this.CheckTextQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextQuestion(expressionProcessorState, userId, questionId,
                rosterVector, answerTime, answer, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }
    }
}