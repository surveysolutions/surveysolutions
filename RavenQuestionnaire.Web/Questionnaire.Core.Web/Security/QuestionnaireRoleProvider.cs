using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using Questionnaire.Core.Web.Helpers;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.User;

namespace Questionnaire.Core.Web.Security
{
    public class QuestionnaireRoleProvider : RoleProvider
    {
        public ICommandService CommandInvoker
        {
            get { return NcqrsEnvironment.Get<ICommandService>();/*KernelLocator.Kernel.Get<ICommandInvoker>()*/; }
        }

        public IViewRepository ViewRepository
        {
            get { return KernelLocator.Kernel.Get<IViewRepository>(); }
        }


        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns><c>true</c> if the specified user name is in the specified role; otherwise, <c>false</c>.</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            string contextKey = "user-in-role:" + username.ToLower() + ":" + roleName;
            bool? retval = (bool?)HttpContext.Current.Items[contextKey];
            if (!retval.HasValue)
            {
                retval = false;
                string[] roles = 
                    this.ViewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(username.ToLower(), null)).Roles.Select(r=>r.ToString()).ToArray();
               
                foreach (string dr in roles)
                    if (dr.Equals(roleName))
                    {
                        retval = true;
                        break;
                    }
                HttpContext.Current.Items.Add(contextKey, retval);
            }
            return retval.Value;
        }



        public override string[] GetRolesForUser(string username)
        {
            return this.ViewRepository.Load<UserViewInputModel, UserView>(
                new UserViewInputModel(username.ToLower() //bad approach
                    , null)).Roles.Select(r=>r.ToString()).ToArray();
        }

        public override void CreateRole(string roleName)
        {
            throw new Exception("CreateRole operation is not allowed.");
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new Exception("DeleteRole operation is not allowed.");
        }

        public override bool RoleExists(string roleName)
        {

            return GetAllRoles().Contains(roleName);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            UserRoles role;
            if (Enum.TryParse(roleName, out role))
                return
                    this.ViewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel(role)
                                                                                       {PageSize = 100}).Items
                        .Select(u => u.UserName).ToArray();
            return new string[0];
        }

        /// <summary>
        /// All roles;
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            return Enum.GetNames(typeof (UserRoles));
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get { return applicationName; }
            set { applicationName = value; }
        }
        private string applicationName = "Questionnaire";
    }
}
