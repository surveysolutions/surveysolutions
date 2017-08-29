using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class InterviewReferencesDenormalizer : AbstractFunctionalEventHandler<InterviewReferences, IReadSideKeyValueStorage<InterviewReferences>>,
        IUpdateHandler<InterviewReferences, InterviewCreated>,
        IUpdateHandler<InterviewReferences, InterviewFromPreloadedDataCreated>,
        IUpdateHandler<InterviewReferences, InterviewOnClientCreated>,
        IUpdateHandler<InterviewReferences, InterviewHardDeleted>
    {
        public InterviewReferencesDenormalizer(IReadSideKeyValueStorage<InterviewReferences> readSideStorage)
            : base(readSideStorage) {}

        public InterviewReferences Update(InterviewReferences state, IPublishedEvent<InterviewCreated> e)
            => CreateInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        public InterviewReferences Update(InterviewReferences state, IPublishedEvent<InterviewFromPreloadedDataCreated> e)
            => CreateInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        public InterviewReferences Update(InterviewReferences state, IPublishedEvent<InterviewOnClientCreated> e)
            => CreateInterviewReferences(e.EventSourceId, e.Payload.QuestionnaireId, e.Payload.QuestionnaireVersion);

        private static InterviewReferences CreateInterviewReferences(Guid interviewId, Guid questionnaireId, long questionnaireVersion)
        {
            return new InterviewReferences(interviewId, questionnaireId, questionnaireVersion);
        }

        public InterviewReferences Update(InterviewReferences state, IPublishedEvent<InterviewHardDeleted> @event) => null;
    }
}