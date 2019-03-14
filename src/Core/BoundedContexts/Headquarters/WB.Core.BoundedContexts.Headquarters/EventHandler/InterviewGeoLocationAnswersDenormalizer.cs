using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewGeoLocationAnswersDenormalizer : BaseDenormalizer,
        IEventHandler<GeoLocationQuestionAnswered>,
        IEventHandler<QuestionsEnabled>,
        IEventHandler<QuestionsDisabled>,
        IEventHandler<AnswersRemoved>
    {
        private readonly IInterviewFactory interviewFactory;

        public InterviewGeoLocationAnswersDenormalizer(IInterviewFactory interviewFactory)
        {
            this.interviewFactory = interviewFactory;
        }

        public override object[] Writers => new object[] { };

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> @event) =>
            this.interviewFactory.SaveGeoLocation(@event.EventSourceId,
                Identity.Create(@event.Payload.QuestionId, @event.Payload.RosterVector), @event.Payload.Latitude,
                @event.Payload.Longitude, @event.Payload.Timestamp);
            

        public void Handle(IPublishedEvent<AnswersRemoved> @event) =>
            this.interviewFactory.RemoveGeoLocations(@event.EventSourceId, @event.Payload.Questions);

        public void Handle(IPublishedEvent<QuestionsEnabled> @event) =>
            this.interviewFactory.EnableGeoLocationAnswers(@event.EventSourceId, @event.Payload.Questions, true);

        public void Handle(IPublishedEvent<QuestionsDisabled> @event) =>
            this.interviewFactory.EnableGeoLocationAnswers(@event.EventSourceId, @event.Payload.Questions, false);
    }
}
