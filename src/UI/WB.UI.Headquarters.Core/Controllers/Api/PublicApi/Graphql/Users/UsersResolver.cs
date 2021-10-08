#nullable enable
using System.Linq;
using HotChocolate;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UsersResolver
    {
        public IQueryable<HqUser> GetUsers(
            [Service] IUnitOfWork? unitOfWork,
            [Service] IAuthorizedUser authorizedUser)
        {
            unitOfWork.DiscardChanges();
            var query = unitOfWork.Session.Query<HqUser>();

            /*if (!authorizedUser.IsAdministrator)
            {
                var authorizedWorkspaces = authorizedUser.Workspaces.ToList();

                query = query
                    .Where(u =>
                        u.Workspaces.Any(w => authorizedWorkspaces.Any(aw => aw == w.Workspace.Name)))
                    /*.Select(u =>
                        u.Workspaces.Where(w => authorizedWorkspaces.Any(aw => aw == w.Workspace.Name)))#1#;
            }*/
            
            return query;
        }
    }
}
