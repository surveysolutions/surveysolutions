using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
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

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            var answeredQuestion = new Identity(questionId, rosterVector);
            CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState);


            var sourceInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);
            var changedInterviewTree = this.BuildInterviewTree(questionnaire, this.interviewState);

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            var answer = new GeoPosition(latitude, longitude, accuracy, altitude, timestamp);
            changedInterviewTree.GetQuestion(answeredQuestion).AsGps.SetAnswer(answer);
            ApplyQuestionAnswer(userId, changedInterviewTree, questionnaire, changedQuestionIdentities, sourceInterviewTree);
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