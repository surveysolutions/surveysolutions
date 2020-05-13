using System;
using Microsoft.Extensions.Caching.Memory;
using Ncqrs.Eventing.Storage;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterEventStore : InMemoryEventStore
    {
        private readonly IAppdomainsPerInterviewManager interviewManager;

        public WebTesterEventStore(IMemoryCache memoryCache, IAppdomainsPerInterviewManager interviewManager) 
            : base(memoryCache)
        {
            this.interviewManager = interviewManager;
        }

        public override int? GetLastEventSequence(Guid id)
        {
            return interviewManager.GetLastEventSequence(id);
        }
    }
}
