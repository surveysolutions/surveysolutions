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
        ApplicationUser CurrentUser { get; }
        IQueryable<ApplicationUser> Users { get; }
        Guid CurrentUserId { get; }
        string CurrentUserName { get; }
        string CurrentUserDeviceId { get; }
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password, UserRoles role);
        IdentityResult CreateUser(ApplicationUser user, string password, UserRoles role);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user, string password);
        IdentityResult UpdateUser(ApplicationUser user, string password);
        Task<SignInStatus> SignInAsync(string userName, string password, bool isPersistent = false);
        Task<UserRoles> GetRoleForCurrentUserAsync();
        void SignOut();
        Task<ApplicationUser> GetUserByIdAsync(Guid userId);
        ApplicationUser GetUserById(Guid userId);
        Task<ApplicationUser> GetUserByNameAsync(string userName);
        ApplicationUser GetUserByName(string userName);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        ApplicationUser GetUserByEmail(string email);
        IEnumerable<IdentityResult> DeleteSupervisorAndDependentInterviewers(Guid supervisorId);
        void LinkDeviceToCurrentInterviewer(string deviceId);
        Task SignInAsObserverAsync(string userName);
        Task SignInBackFromObserverAsync();
        RestCredentials GetDesignerUserCredentials();
        bool IsUserValidWithPassword(string userName, string password);
        bool IsUserValidWithPasswordHash(string userName, string passwordHash);
        Task<IdentityResult[]> ArchiveUsersAsync(Guid[] userIds, bool archive);
    }
}