using System;
using System.Linq;
using Microsoft.AspNet.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewNotificationService : IWebInterviewNotificationService
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;

        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
        }

        private static readonly Lazy<IHubContext> _webInterviewHubInstance = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<WebInterview>());

        protected IHubContext HubContext => _webInterviewHubInstance.Value;
        
        public void RefreshEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var entitiesToRefresh = entities.Select(identity => Tuple.Create(GetClientGroupIdentity(identity, interview), identity));

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
            {
                if (questionsGroupedByParent.Key == null)
                {
                    HubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
                    continue;
                };

                HubContext.Clients.Group(questionsGroupedByParent.Key).refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()));
            }
        }

        private static string GetClientGroupIdentity(Identity identity, IStatefulInterview interview)
        {
            var questionToRefresh = interview.GetQuestion(identity);
            if (questionToRefresh == null) return null;

            var sectionKey = questionToRefresh.IsPrefilled
                ? string.Empty
                : questionToRefresh.Parent.Identity.ToString();
            
            return WebInterview.GetConnectedClientSectionKey(sectionKey, interview.Id.FormatGuid());
        }
    }
}