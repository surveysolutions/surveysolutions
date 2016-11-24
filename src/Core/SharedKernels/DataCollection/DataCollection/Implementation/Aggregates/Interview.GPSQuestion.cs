using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerGeoLocationQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, double latitude, double longitude,
            double accuracy, double altitude, DateTimeOffset timestamp)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow();
            var answeredQuestion = new Identity(questionId, rosterVector);

            this.CheckGpsCoordinatesInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.Tree);
          
            var changedInterviewTree = this.Tree.Clone();

            var changedQuestionIdentities = new List<Identity> { answeredQuestion };
            var answer = new GeoPosition(latitude, longitude, accuracy, altitude, timestamp);
            changedInterviewTree.GetQuestion(answeredQuestion).AsGps.SetAnswer(GpsAnswer.FromGeoPosition(answer));
            this.CalculateTreeDiffChanges(changedInterviewTree, questionnaire, changedQuestionIdentities);

            this.ApplyEvents(changedInterviewTree, userId);
        }
    }
}