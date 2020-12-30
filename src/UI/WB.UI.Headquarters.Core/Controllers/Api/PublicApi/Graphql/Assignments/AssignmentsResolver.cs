#nullable enable
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Assignments
{
    public class AssignmentsResolver
    {
        public IQueryable<Core.BoundedContexts.Headquarters.Assignments.Assignment> Assignments(
            [Service] IUnitOfWork unitOfWork, [Service] IAuthorizedUser user)
        {
            unitOfWork.DiscardChanges();

            var assignments = 
                unitOfWork.Session.Query<Core.BoundedContexts.Headquarters.Assignments.Assignment>();

            if (user.IsSupervisor)
            {
                assignments = assignments
                    .Where(x => x.Responsible.ReadonlyProfile.SupervisorId == user.Id || x.ResponsibleId == user.Id);
            }

            if (user.IsInterviewer)
            {
                assignments = assignments.Where(x => x.ResponsibleId == user.Id);
            }

            return assignments;
        }
    }
}
