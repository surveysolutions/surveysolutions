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
        bool IsApiUser { get; }
        bool IsObserver { get; }
        bool IsObserving { get; }
        bool IsAuthenticated { get; }

        Guid Id { get; }
        string UserName { get; }
        bool NeedChangePassword { get; }
        bool HasNonDefaultWorkspace { get; }
        IEnumerable<string> Workspaces { get; }

        /// <summary>
        /// Check that Authorized User has access to specified workspace
        /// </summary>
        /// <param name="workspace">Workspace name to check against</param>
        /// <returns>Return true if user assigned to workspace, even for disabled one</returns>
        bool HasAccessToWorkspace(string workspace);
        IEnumerable<WorkspaceContext> GetEnabledWorkspaces();
        void ResetForceChangePasswordFlag();
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
