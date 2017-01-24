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
        private readonly IHubContext webInterviewHubContext;

        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository, [Ninject.Named("WebInterview")] IHubContext webInterviewHubContext)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewHubContext = webInterviewHubContext;
        }

        public void RefreshEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var entitiesToRefresh = entities.Select(identity => Tuple.Create(GetClientGroupIdentity(identity, interview), identity));

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
            {
                if (questionsGroupedByParent.Key == null)
                {
                    webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
                    continue;
                };

                webInterviewHubContext.Clients.Group(questionsGroupedByParent.Key).refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()));
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