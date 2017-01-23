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

        // Singleton instance
        private static readonly Lazy<IHubContext> _instance = new Lazy<IHubContext>(
            () => GlobalHost.ConnectionManager.GetHubContext<WebInterview>());

        public void RefreshEntities(Guid interviewId, params Identity[] entities)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            var refreshLists = from entity in entities
                     let key = GetRefreshKey(interview, entity)
                     where key != null
                     group entity by key into g
                     select new
                     {
                         Key = g.Key,
                         Questions = g.Select(q => q.ToString()).ToArray()
                     };
            
            foreach(var list in refreshLists)
            {
                _instance.Value.Clients.Group(list.Key).refreshEntities(list.Questions);
            }            
        }

        private string GetRefreshKey(IStatefulInterview interview, Identity entity)
        {
            var question = interview.GetQuestion(entity);
            if (question == null) return null;

            var prefilled = interview.IsQuestionPrefilled(entity);
            if (prefilled)
            {
                return "_" + interview.Id.FormatGuid();
            }

            var parent = interview.GetParentGroup(entity);
            return $"{parent?.ToString() ?? ""}_{interview.Id.FormatGuid()}";
        }
    }
}