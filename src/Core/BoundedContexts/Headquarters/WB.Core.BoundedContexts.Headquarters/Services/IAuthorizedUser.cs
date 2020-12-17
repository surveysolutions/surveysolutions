using System;
using System.Collections.Generic;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAuthorizedUser
    {
        bool IsInterviewer { get; }
        bool IsAdministrator { get; }
        bool IsHeadquarter { get; }
        bool IsSupervisor { get; }
        bool IsObserver { get; }
        bool IsObserving { get; }
        bool IsAuthenticated { get; }

        Guid Id { get; }
        string UserName { get; }
        bool HasNonDefaultWorkspace { get; }
        IEnumerable<string> Workspaces { get; }
        bool HasAccessToWorkspace(string targetWorkspace);
        IEnumerable<WorkspaceContext> GetEnabledWorkspaces();
    }

    public static class AuthorizedUserHelpers
    {
        public static bool CanConductInterviewReview(this IAuthorizedUser authorizedUser)
        {
            return authorizedUser.IsAuthenticated && (
                       authorizedUser.IsAdministrator 
                       || authorizedUser.IsHeadquarter 
                       || authorizedUser.IsSupervisor
            );
        }
    }
}
