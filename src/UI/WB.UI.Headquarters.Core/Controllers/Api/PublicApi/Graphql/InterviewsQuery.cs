using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public class InterviewsQuery
    {
        public IQueryable<InterviewSummary> GetInterviews([Service] IUnitOfWork unitOfWork)
        {
            unitOfWork.DiscardChanges();
            
            return unitOfWork.Session.Query<InterviewSummary>();
        }
    }
}
