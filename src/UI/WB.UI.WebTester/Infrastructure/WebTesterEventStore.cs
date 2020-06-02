using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.Aggregates;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterEventStore : InMemoryEventStore
    {
        private readonly IAppdomainsPerInterviewManager interviewManager;

        public WebTesterEventStore(
            IAggregateRootCache aggregateRootCache, 
            IAppdomainsPerInterviewManager interviewManager) 
            : base(aggregateRootCache)
        {
            this.interviewManager = interviewManager;
        }

        public override int? GetLastEventSequence(Guid id)
        {
            return interviewManager.GetLastEventSequence(id);
        }
    }
}
