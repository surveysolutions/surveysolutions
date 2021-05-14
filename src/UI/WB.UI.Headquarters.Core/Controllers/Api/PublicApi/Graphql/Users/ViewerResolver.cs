#nullable enable
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class ViewerResolver
    {
        public UserDto GetViewer([Service] IAuthorizedUser authorizedUser)
        {
            var roles = new List<UserRoles>();
            
            if(authorizedUser.IsAdministrator) roles.Add(UserRoles.Administrator);
            if(authorizedUser.IsHeadquarter) roles.Add(UserRoles.Headquarter);
            if(authorizedUser.IsInterviewer) roles.Add(UserRoles.Interviewer);
            if(authorizedUser.IsSupervisor) roles.Add(UserRoles.Supervisor);
            if(authorizedUser.IsApiUser) roles.Add(UserRoles.ApiUser);
            if(authorizedUser.IsObserver) roles.Add(UserRoles.Observer);

            return new UserDto
            {
                Id = authorizedUser.Id,
                UserName = authorizedUser.UserName,
                Role = roles[0],
                Workspaces = authorizedUser.Workspaces.ToArray()
            };
        }
    }
}
