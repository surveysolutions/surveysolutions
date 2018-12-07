using System;
using System.Linq;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.EventHandler;

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
        
        public InterviewStateFixer(IEventStore eventStore,  InterviewDenormalizer denormalizer)
        {
            this.eventStore = eventStore;
            this.denormalizer = denormalizer;
        }

        public void RefreshInterview(Guid interviewId)
        {
            var events = this.eventStore.Read(interviewId, 0).ToList();
            denormalizer.Handle(events, interviewId);
        }
    }
}
