#nullable enable
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class UsersResolver
    {
        public UserDto GetViewer([Service] IAuthorizedUser authorizedUser)
        {
            var roles = new List<UserRoles>();
            
            if(authorizedUser.IsAdministrator) roles.Add(UserRoles.Administrator);
            if(authorizedUser.IsHeadquarter) roles.Add(UserRoles.Headquarter);
            if(authorizedUser.IsInterviewer) roles.Add(UserRoles.Interviewer);
            if(authorizedUser.IsSupervisor) roles.Add(UserRoles.Supervisor);

            return new UserDto
            {
                Id = authorizedUser.Id,
                UserName = authorizedUser.UserName,
                Roles = roles.ToArray(),
                Workspaces = authorizedUser.Workspaces.ToArray()
            };
        }
    }
}
