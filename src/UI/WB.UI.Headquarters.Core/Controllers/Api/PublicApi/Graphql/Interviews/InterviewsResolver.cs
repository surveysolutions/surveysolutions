using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsResolver
    {
        public IQueryable<InterviewSummary> GetInterviews(
            int? skip, int? take,
            [Service] IUnitOfWork unitOfWork, [Service]IAuthorizedUser user)
        {
            unitOfWork.DiscardChanges();

            IQueryable<InterviewSummary> interviewSummaries = unitOfWork.Session.Query<InterviewSummary>();
            if (user.IsSupervisor)
            {
                interviewSummaries = interviewSummaries.Where(x => x.TeamLeadId == user.Id || x.ResponsibleId == user.Id);
            }

            if (user.IsInterviewer)
            {
                interviewSummaries = interviewSummaries.Where(x => x.ResponsibleId == user.Id);
            }
            
            
            if(skip.HasValue)
            {
                interviewSummaries = interviewSummaries.Skip(skip.Value);
            }

            if(take.HasValue)
            {
                interviewSummaries = interviewSummaries.Take(take.Value);
            }
            
            return interviewSummaries;
        }
    }
}
