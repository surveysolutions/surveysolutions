#nullable enable
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews
{
    public class InterviewsResolver
    {
        private static InterviewStatus[] NotForSupervisor =
        {
            InterviewStatus.ApprovedByHeadquarters, InterviewStatus.ApprovedBySupervisor
        };
        
        public IQueryable<InterviewSummary> GetInterviews([Service] IUnitOfWork unitOfWork, [Service]IAuthorizedUser user)
        {
            unitOfWork.DiscardChanges();

            IQueryable<InterviewSummary> interviewSummaries = unitOfWork.Session.Query<InterviewSummary>();
            
            if (user.IsSupervisor)
            {
                interviewSummaries = interviewSummaries
                    .Where(x => !NotForSupervisor.Contains(x.Status)
                                && (x.SupervisorId == user.Id || x.ResponsibleId == user.Id));

            }

            if (user.IsInterviewer)
            {
                interviewSummaries = interviewSummaries.Where(x => x.ResponsibleId == user.Id);
            }
            
            return interviewSummaries;
        }
    }
}
