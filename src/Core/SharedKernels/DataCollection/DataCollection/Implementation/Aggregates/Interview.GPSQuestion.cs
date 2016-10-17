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
        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = CalculateInterviewChangesOnAnswerGeoLocationQuestion(expressionProcessorState, userId, questionId,
                rosterVector, answerTime, latitude, longitude, accuracy, altitude, timestamp, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerGeoLocationQuestion(ILatestInterviewExpressionState expressionProcessorState, Guid userId,
            Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude, double accuracy, double altitude, DateTimeOffset timestamp,
            IQuestionnaire questionnaire)
        {
            expressionProcessorState.UpdateGeoLocationAnswer(questionId, rosterVector, latitude, longitude, accuracy, altitude);

            return this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector,
                new GeoLocationPoint(latitude, longitude, accuracy, altitude, timestamp),
                AnswerChangeType.GeoLocation, answerTime, questionnaire, expressionProcessorState);
        }
    }
}