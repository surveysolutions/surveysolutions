// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireRoleProvider.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire role provider.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.Core.View.User;

namespace Questionnaire.Core.Web.Security
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;

    using Microsoft.Practices.ServiceLocation;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Ninject;

    using Questionnaire.Core.Web.Helpers;

    /// <summary>
    /// The questionnaire role provider.
    /// </summary>
    public class QuestionnaireRoleProvider : RoleProvider
    {
        #region Fields

        /// <summary>
        /// The application name.
        /// </summary>
        private string applicationName = "Questionnaire";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
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

        /// <summary>
        /// Gets the command invoker.
        /// </summary>
        public ICommandService CommandInvoker
        {
            get
            {
                return NcqrsEnvironment.Get<ICommandService>(); /*KernelLocator.Kernel.Get<ICommandInvoker>()*/
                ;
            }
        }

        /// <summary>
        /// Gets the view repository.
        /// </summary>
        public IViewRepository ViewRepository
        {
            get
            {
                return ServiceLocator.Current.GetInstance<IViewRepository>();
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add users to roles.
        /// </summary>
        /// <param name="usernames">
        /// The usernames.
        /// </param>
        /// <param name="roleNames">
        /// The role names.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The create role.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <exception cref="Exception">
        /// </exception>
        public override void CreateRole(string roleName)
        {
            throw new Exception("CreateRole operation is not allowed.");
        }

        /// <summary>
        /// The delete role.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <param name="throwOnPopulatedRole">
        /// The throw on populated role.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new Exception("DeleteRole operation is not allowed.");
        }

        /// <summary>
        /// The find users in role.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <param name="usernameToMatch">
        /// The username to match.
        /// </param>
        /// <returns>
        /// The System.String[].
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// All roles;
        /// </summary>
        /// <returns>
        /// The System.String[].
        /// </returns>
        public override string[] GetAllRoles()
        {
            return Enum.GetNames(typeof(UserRoles));
        }

        /// <summary>
        /// The get roles for user.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <returns>
        /// The System.String[].
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {
            UserView user =
                this.ViewRepository.Load<UserViewInputModel, UserView>(
                    new UserViewInputModel(
                        username.ToLower() // bad approach
                        , 
                        null));
            if (user == null)
            {
                return new string[0];
            }

            return user.Roles.Select(r => r.ToString()).ToArray();
        }

        /// <summary>
        /// The get users in role.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <returns>
        /// The System.String[].
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            UserRoles role;
            if (Enum.TryParse(roleName, out role))
            {
                return
                    this.ViewRepository.Load<UserBrowseInputModel, UserBrowseView>(
                        new UserBrowseInputModel(role) { PageSize = 100 }).Items.Select(u => u.UserName).ToArray();
            }

            return new string[0];
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role.
        /// </summary>
        /// <param name="username">
        /// The user name to search for.
        /// </param>
        /// <param name="roleName">
        /// The role to search in.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified user name is in the specified role; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            string contextKey = "user-in-role:" + username.ToLower() + ":" + roleName;
            var retval = (bool?)HttpContext.Current.Items[contextKey];
            if (!retval.HasValue)
            {
                retval = false;
                string[] roles =
                    this.ViewRepository.Load<UserViewInputModel, UserView>(
                        new UserViewInputModel(username.ToLower(), null)).Roles.Select(r => r.ToString()).ToArray();

                foreach (string dr in roles)
                {
                    if (dr.Equals(roleName))
                    {
                        retval = true;
                        break;
                    }
                }

                HttpContext.Current.Items.Add(contextKey, retval);
            }

            return retval.Value;
        }

        /// <summary>
        /// The remove users from roles.
        /// </summary>
        /// <param name="usernames">
        /// The usernames.
        /// </param>
        /// <param name="roleNames">
        /// The role names.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The role exists.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            return this.GetAllRoles().Contains(roleName);
        }

        #endregion
    }
}