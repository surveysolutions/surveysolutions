using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewNotificationService : IWebInterviewNotificationService
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IHubContext webInterviewHubContext;

        public WebInterviewNotificationService(IStatefulInterviewRepository statefulInterviewRepository,
            [Ninject.Named("WebInterview")] IHubContext webInterviewHubContext)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.webInterviewHubContext = webInterviewHubContext;
        }

        public void RefreshEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var entitiesToRefresh = entities.Select(identity => Tuple.Create(this.GetClientGroupIdentity(identity, interview), identity));

            foreach (var questionsGroupedByParent in entitiesToRefresh.GroupBy(x => x.Item1))
            {
                if (questionsGroupedByParent.Key == null)
                {
                    continue;
                }

                var clients = webInterviewHubContext.Clients;
                var group = clients.Group(questionsGroupedByParent.Key);

                group.refreshEntities(questionsGroupedByParent.Select(p => p.Item2.ToString()).ToArray());
            }

            webInterviewHubContext.Clients.Group(interviewId.FormatGuid()).refreshSection();
        }

        private Identity GetParentIdentity(Identity identity, IStatefulInterview interview)
        {
            return ((IInterviewTreeNode) interview.GetQuestion(identity)
                    ?? (IInterviewTreeNode) interview.GetStaticText(identity)
                    ?? (IInterviewTreeNode) interview.GetRoster(identity)
                    ?? (IInterviewTreeNode) interview.GetGroup(identity))
                ?.Parent?.Identity;
        }

        private string GetClientGroupIdentity(Identity identity, IStatefulInterview interview)
        {
            string sectionKey;

            if (interview.GetQuestion(identity)?.IsPrefilled ?? false)
            {
                sectionKey = null;
            }
            else
            {
                sectionKey = this.GetParentIdentity(identity, interview)?.ToString();
            }

            return WebInterview.GetConnectedClientSectionKey(sectionKey, interview.Id.FormatGuid());
        }
    }
}