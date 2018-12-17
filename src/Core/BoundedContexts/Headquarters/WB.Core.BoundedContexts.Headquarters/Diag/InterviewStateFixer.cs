using System;
using System.Linq;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Diag
{
    public interface IInterviewStateFixer
    {
        void RefreshInterview(Guid interviewId);
    }

    internal class InterviewStateFixer : IInterviewStateFixer
    {
        private readonly IEventStore eventStore;
        private readonly InterviewDenormalizer denormalizer;
        private readonly IInterviewFactory interviewFactory;

        public InterviewStateFixer(IEventStore eventStore,  InterviewDenormalizer denormalizer, IInterviewFactory interviewFactory)
        {
            this.eventStore = eventStore;
            this.denormalizer = denormalizer;
            this.interviewFactory = interviewFactory;
        }

        public void RefreshInterview(Guid interviewId)
        {
            this.interviewFactory.RemoveInterview(interviewId);
            var events = this.eventStore.Read(interviewId, 0).ToList();
            denormalizer.Handle(events, interviewId);
        }
    }
}
