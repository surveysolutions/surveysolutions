using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security
{
    public class QuestionnaireRoleProvider : RoleProvider
    {
        private string applicationName = "Questionnaire";

        private IUserWebViewFactory UserViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IUserWebViewFactory>(); }
        }

        private IViewFactory<UserBrowseInputModel, UserBrowseView> UserBrowseViewFactory
        {
            get { return ServiceLocator.Current.GetInstance<IViewFactory<UserBrowseInputModel, UserBrowseView>>(); }
        }

        private ITransactionManagerProvider TransactionProvider
        {
            get { return ServiceLocator.Current.GetInstance<ITransactionManagerProvider>(); }
        }

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
            throw new Exception("CreateRole operation is not allowed.");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new Exception("DeleteRole operation is not allowed.");
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
            var transactionManager = this.TransactionProvider.GetTransactionManager();
            var shouldUseOwnTransaction = !transactionManager.IsQueryTransactionStarted;

            if (shouldUseOwnTransaction)
            {
                transactionManager.BeginQueryTransaction();
            }


            try
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
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    transactionManager.RollbackQueryTransaction();
                }
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            UserRoles role;
            if (Enum.TryParse(roleName, out role))
            {
                return this.UserBrowseViewFactory.Load(new UserBrowseInputModel(role) { PageSize = 100 }).Items.Select(u => u.UserName).ToArray();
            }

            return new string[0];
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            string contextKey = "user-in-role:" + username.ToLower() + ":" + roleName;
            var cachedValue = (bool?)HttpContext.Current.Items[contextKey];

            if (cachedValue.HasValue)
                return cachedValue.Value;

            var transactionManager = this.TransactionProvider.GetTransactionManager();
            var shouldUseOwnTransaction = !transactionManager.IsQueryTransactionStarted;

            if (shouldUseOwnTransaction)
            {
                transactionManager.BeginQueryTransaction();
            }

            try
            {
                UserWebView user = this.UserViewFactory.Load(new UserWebViewInputModel(username.ToLower(), null));
                
                bool hasRole = user.Roles.Any(role => role.ToString().Equals(roleName));

                HttpContext.Current.Items.Add(contextKey, hasRole);

                return hasRole;
            }
            finally
            {
                if (shouldUseOwnTransaction)
                {
                    transactionManager.RollbackQueryTransaction();
                }
            }
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