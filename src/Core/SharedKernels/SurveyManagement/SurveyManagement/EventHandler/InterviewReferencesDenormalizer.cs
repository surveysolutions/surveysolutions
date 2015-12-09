using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewReferencesDenormalizer : BaseDenormalizer,
        IEventHandler<InterviewCreated>,
        IEventHandler<InterviewFromPreloadedDataCreated>,
        IEventHandler<InterviewOnClientCreated>
    {
        private readonly IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage;

        public InterviewReferencesDenormalizer(IReadSideKeyValueStorage<InterviewReferences> interviewReferencesStorage)
        {
            this.interviewReferencesStorage = interviewReferencesStorage;
        }

        public override object[] Writers => new object[] { this.interviewReferencesStorage };

        public void Handle(IPublishedEvent<InterviewCreated> e) => this.StoreInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> e) => this.StoreInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        public void Handle(IPublishedEvent<InterviewOnClientCreated> e) => this.StoreInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        private void StoreInterviewReferences(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewReferences = new InterviewReferences
            {
                InterviewId = interviewId,
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
            };

            this.interviewReferencesStorage.Store(interviewReferences, interviewId);
        }
    }
}