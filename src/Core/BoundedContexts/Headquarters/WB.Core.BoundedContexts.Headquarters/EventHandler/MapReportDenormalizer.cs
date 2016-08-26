using System;
using System.Linq;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class MapReportDenormalizer : BaseDenormalizer,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<AnswersRemoved>,
        IEventHandler<InterviewDeleted>,
        IEventHandler<InterviewHardDeleted>
    {
        private readonly IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage;
        private readonly IReadSideRepositoryWriter<MapReportPoint> mapReportPointStorage;

        public override object[] Writers => new object[] { this.mapReportPointStorage };

        public override object[] Readers => new object[] { this.interviewReferencesStorage};

        public MapReportDenormalizer(
            IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage,
            IReadSideRepositoryWriter<MapReportPoint> mapReportPointStorage, 
            IQuestionnaireStorage questionnaireStorage,
            IPlainKeyValueStorage<QuestionnaireQuestionsInfo> questionnaireQuestionsInfoStorage)
        {
            if (interviewReferencesStorage == null) throw new ArgumentNullException(nameof(interviewReferencesStorage));
            if (mapReportPointStorage == null) throw new ArgumentNullException(nameof(mapReportPointStorage));

            this.interviewReferencesStorage = interviewReferencesStorage;
            this.mapReportPointStorage = mapReportPointStorage;
            this.questionnaireStorage = questionnaireStorage;
            this.questionnaireQuestionsInfoStorage = questionnaireQuestionsInfoStorage;
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            Guid interviewId = evnt.EventSourceId;
            InterviewReferences interviewReferences = this.interviewReferencesStorage.GetById(interviewId);
            if (interviewReferences == null) return;

            var variableName = this.GetVariableNameForQuestion(evnt.Payload.QuestionId, 
                new QuestionnaireIdentity(interviewReferences.QuestionnaireId, interviewReferences.QuestionnaireVersion));

            var mapPointKey = GetMapReportPointId(interviewId, variableName, evnt.Payload.RosterVector);

            MapReportPoint mapReportAnswersCollection = this.mapReportPointStorage.GetById(mapPointKey) ?? new MapReportPoint(mapPointKey);

            mapReportAnswersCollection.Latitude = evnt.Payload.Latitude;
            mapReportAnswersCollection.Longitude = evnt.Payload.Longitude;
            mapReportAnswersCollection.InterviewId = interviewId;
            mapReportAnswersCollection.QuestionnaireId = interviewReferences.QuestionnaireId;
            mapReportAnswersCollection.QuestionnaireVersion = interviewReferences.QuestionnaireVersion;
            mapReportAnswersCollection.Variable = variableName;
            
            this.mapReportPointStorage.Store(mapReportAnswersCollection, mapPointKey);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.HandleSingleRemove(evnt.EventSourceId, question.Id, question.RosterVector);
            }
        }

        private void HandleSingleRemove(Guid interviewId, Guid questionId, RosterVector rosterVector)
        {
            InterviewReferences interviewReferences = this.interviewReferencesStorage.GetById(interviewId);
            if (interviewReferences == null) return;

            var variableName = this.GetVariableNameForQuestion(questionId,
                new QuestionnaireIdentity(interviewReferences.QuestionnaireId, interviewReferences.QuestionnaireVersion));

            var mapPointKey = GetMapReportPointId(interviewId, variableName, rosterVector);

            this.mapReportPointStorage.Remove(mapPointKey);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt) => this.ClearMapReportPointsForInterview(evnt.EventSourceId);

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt) => this.ClearMapReportPointsForInterview(evnt.EventSourceId);

        private void ClearMapReportPointsForInterview(Guid interviewId)
        {
            this.mapReportPointStorage.RemoveIfStartsWith(interviewId.ToString());
        }

        private static string GetMapReportPointId(Guid interviewId, string variableName, RosterVector rosterVector)
            => $"{interviewId}-{variableName}-{rosterVector}";

        private string GetVariableNameForQuestion(Guid questionId, QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireQuestionsInfo = this.questionnaireQuestionsInfoStorage.GetById(questionnaireIdentity.ToString());
            return questionnaireQuestionsInfo.QuestionIdToVariableMap[questionId];
        }
    }
}