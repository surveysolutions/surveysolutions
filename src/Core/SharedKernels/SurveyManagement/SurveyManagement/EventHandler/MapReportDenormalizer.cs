using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class MapReportDenormalizer : BaseDenormalizer,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<AnswersRemoved>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionsInfo;
        private readonly IReadSideRepositoryWriter<MapReportPoint> mapItems;
        private readonly IReadSideRepositoryWriter<InterviewSummary> interviewBriefStorage;

        public override object[] Writers => new object[] { this.mapItems };

        public override object[] Readers => new object[] { this.interviewBriefStorage, this.questionsInfo };

        public MapReportDenormalizer(IReadSideRepositoryWriter<InterviewSummary> interviewBriefStorage,
            IReadSideKeyValueStorage<QuestionnaireQuestionsInfo> questionsInfo, 
            IReadSideRepositoryWriter<MapReportPoint> mapItems)
        {
            if (interviewBriefStorage == null) throw new ArgumentNullException(nameof(interviewBriefStorage));
            if (questionsInfo == null) throw new ArgumentNullException(nameof(questionsInfo));
            if (mapItems == null) throw new ArgumentNullException(nameof(mapItems));

            this.interviewBriefStorage = interviewBriefStorage;
            this.questionsInfo = questionsInfo;
            this.mapItems = mapItems;
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            Guid interviewId = evnt.EventSourceId;
            InterviewSummary interviewSummary = this.interviewBriefStorage.GetById(interviewId);
            if (interviewSummary == null) return;

            var variableName = this.GetVariableNameForQuestion(evnt.Payload.QuestionId, 
                new QuestionnaireIdentity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));

            var targetVector = new RosterVector(evnt.Payload.RosterVector).ToString();
            var mapPointKey = GetMapPointKey(interviewId, variableName, targetVector);

            MapReportPoint mapReportAnswersCollection = this.mapItems.GetById(mapPointKey) ?? new MapReportPoint(mapPointKey);

            mapReportAnswersCollection.Latitude = evnt.Payload.Latitude;
            mapReportAnswersCollection.Longitude = evnt.Payload.Longitude;
            mapReportAnswersCollection.InterviewId = interviewId;
            mapReportAnswersCollection.QuestionnaireId = interviewSummary.QuestionnaireId;
            mapReportAnswersCollection.QuestionnaireVersion = interviewSummary.QuestionnaireVersion;
            mapReportAnswersCollection.Variable = variableName;
            
            this.mapItems.Store(mapReportAnswersCollection, mapPointKey);
        }

        private static string GetMapPointKey(Guid interviewId, string variableName, string targetVector)
        {
            string mapPointKey = $"{interviewId}-{variableName}-{targetVector}";
            return mapPointKey;
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.HandleSingleRemove(evnt.EventSourceId, question.Id, question.RosterVector);
            }
        }

        private void HandleSingleRemove(Guid interviewId, Guid questionId, decimal[] propagationVector)
        {
            InterviewSummary interviewSummary = this.interviewBriefStorage.GetById(interviewId);
            if (interviewSummary == null) return;

            var variableName = this.GetVariableNameForQuestion(questionId,
                new QuestionnaireIdentity(interviewSummary.QuestionnaireId, interviewSummary.QuestionnaireVersion));

            var targetVector = new RosterVector(propagationVector).ToString();
            var mapPointKey = GetMapPointKey(interviewId, variableName, targetVector);

            this.mapItems.Remove(mapPointKey);
        }

        private string GetVariableNameForQuestion(Guid questionId, QuestionnaireIdentity questionnaireId)
        {
            var questionnaireQuestionsInfo = this.questionsInfo.GetById(questionnaireId.ToString());
            return questionnaireQuestionsInfo.QuestionIdToVariableMap[questionId];
        }
    }
}