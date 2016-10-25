using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.UI.Headquarters.Models.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security
{
    public class QuestionnaireRoleProvider : RoleProvider
    {
        private string applicationName = "Questionnaire";

        private IUserWebViewFactory UserViewFactory => ServiceLocator.Current.GetInstance<IUserWebViewFactory>();

        private IUserBrowseViewFactory UserBrowseViewFactory => ServiceLocator.Current.GetInstance<IUserBrowseViewFactory>();

        private IPlainTransactionManager PlainTransactionManager => ServiceLocator.Current.GetInstance<IPlainTransactionManagerProvider>().GetPlainTransactionManager();

        private IReadSideStatusService readSideStatusService => ServiceLocator.Current.GetInstance<IReadSideStatusService>();

        public override string ApplicationName
        {
            get
            {
                return this.applicationName;
            }

            set
            {
                this.applicationName = value;
            }
        }

        public ICommandService CommandInvoker
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ICommandService>(); /*KernelLocator.Kernel.Get<ICommandInvoker>()*/
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new Exception("Operation is not allowed.");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new Exception("Operation is not allowed.");
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            return Enum.GetNames(typeof(UserRoles));
        }

        public override string[] GetRolesForUser(string username)
        {
            if (readSideStatusService.AreViewsBeingRebuiltNow())
                return new string[0];

            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                UserWebView user =
                    this.UserViewFactory.Load(
                        new UserWebViewInputModel(
                            username.ToLower() // bad approach
                            ,
                            null));
                if (user == null)
                {
                    return new string[0];
                }

                return user.Roles.Select(r => r.ToString()).ToArray();
            });
        }

        public override string[] GetUsersInRole(string roleName)
        {
            UserRoles role;
            if (Enum.TryParse(roleName, out role))
            {
                return this.PlainTransactionManager.ExecuteInPlainTransaction(() => 
                     this.UserBrowseViewFactory.Load(new UserBrowseInputModel(role) { PageSize = 100 }).Items.Select(u => u.UserName).ToArray());
            }

            return new string[0];
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            if (readSideStatusService.AreViewsBeingRebuiltNow())
                return false;

            string contextKey = "user-in-role:" + username.ToLower() + ":" + roleName;
            var cachedValue = (bool?)HttpContext.Current.Items[contextKey];

            if (cachedValue.HasValue)
                return cachedValue.Value;
            return this.PlainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                UserWebView user = this.UserViewFactory.Load(new UserWebViewInputModel(username.ToLower(), null));

                bool hasRole = user.Roles.Any(role => role.ToString().Equals(roleName));

                HttpContext.Current.Items.Add(contextKey, hasRole);

                return hasRole;
            });
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            return this.GetAllRoles().Contains(roleName);
        }
    }
}