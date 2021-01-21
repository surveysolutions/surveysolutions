#nullable enable
using Main.Core.Entities.SubEntities;
using MediatR;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Code.UsersManagement
{
    public class UsersManagementRequest : DataTableRequest, IRequest<DataTableResponse<UserManagementListItem>>
    {
        public string? WorkspaceName { get; set; }
        public UserRoles? Role { get; set; }
        public UserManagementFilter Filter { get; set; }
    }

    public enum UserManagementFilter
    {
        AllUsers,
        WithMissingWorkspace,
        WithDisabledWorkspaces,
        Locked,
        Archived
    }
}
