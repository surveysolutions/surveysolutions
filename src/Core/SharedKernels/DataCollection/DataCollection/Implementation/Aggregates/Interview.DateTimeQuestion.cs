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
        private InterviewChanges CalculateInterviewChangesOnAnswerDateTimeQuestion(ILatestInterviewExpressionState expressionProcessorState, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer, IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateDateAnswer(questionId, rosterVector, answer);

            return this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, answer, AnswerChangeType.DateTime,
                answerTime, questionnaire, expressionProcessorState);
        }

        public void AnswerDateTimeQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, DateTime answer)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            this.CheckDateTimeQuestionInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerDateTimeQuestion(expressionProcessorState, userId,
                questionId, rosterVector, answerTime, answer, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }
    }
}