using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IIdentityManager
    {
        bool HasAdministrator { get; }
        bool IsCurrentUserAdministrator { get; }
        bool IsCurrentUserHeadquarter { get; }
        bool IsCurrentUserSupervisor { get; }
        bool IsCurrentUserObserver { get; }
        HqUser CurrentUser { get; }
        IQueryable<HqUser> Users { get; }
        Guid CurrentUserId { get; }
        string CurrentUserName { get; }
        string CurrentUserDeviceId { get; }
        Task<IdentityResult> CreateUserAsync(HqUser user, string password, UserRoles role);
        IdentityResult CreateUser(HqUser user, string password, UserRoles role);
        Task<IdentityResult> UpdateUserAsync(HqUser user, string password);
        IdentityResult UpdateUser(HqUser user, string password);
        Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false);
        Task<UserRoles> GetRoleForCurrentUserAsync();
        void SignOut();
        Task<HqUser> GetUserByIdAsync(Guid userId);
        HqUser GetUserById(Guid userId);
        Task<HqUser> GetUserByNameAsync(string userName);
        HqUser GetUserByName(string userName);
        Task<HqUser> GetUserByEmailAsync(string email);
        HqUser GetUserByEmail(string email);
        IEnumerable<IdentityResult> DeleteSupervisorAndDependentInterviewers(Guid supervisorId);
        void LinkDeviceToCurrentInterviewer(string deviceId);
        Task SignInAsObserverAsync(string userName);
        Task SignInBackFromObserverAsync();
        RestCredentials GetDesignerUserCredentials();
        Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive);
    }
}