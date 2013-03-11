using System.Web.Security;
using Ninject;
using Questionnaire.Core.Web.Helpers;

namespace RavenQuestionnaire.Web.Tests.Stubs
{
    using Microsoft.Practices.ServiceLocation;

    public interface IRoleProviderMock
    {
        bool IsUserInRole(string username, string roleName);
        string[] GetRolesForUser(string username);
        void CreateRole(string roleName);
        bool DeleteRole(string roleName, bool throwOnPopulatedRole);
        bool RoleExists(string roleName);
        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void RemoveUsersFromRoles(string[] usernames, string[] roleNames);
        string[] GetUsersInRole(string roleName);
        string[] GetAllRoles();
        string[] FindUsersInRole(string roleName, string usernameToMatch);
        string ApplicationName { get; set; }
    }

    public class TestRoleProvider : RoleProvider
    {
        protected IRoleProviderMock LogicMockObject
        {
            get { return ServiceLocator.Current.GetInstance<IRoleProviderMock>(); }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return LogicMockObject.IsUserInRole(username, roleName);
        }

        public override string[] GetRolesForUser(string username)
        {
            return LogicMockObject.GetRolesForUser(username);
        }

        public override void CreateRole(string roleName)
        {
            LogicMockObject.CreateRole(roleName);
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            return LogicMockObject.DeleteRole(roleName, throwOnPopulatedRole);
        }

        public override bool RoleExists(string roleName)
        {
            return LogicMockObject.RoleExists(roleName);
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            LogicMockObject.AddUsersToRoles(usernames, roleNames);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            LogicMockObject.RemoveUsersFromRoles(usernames, roleNames);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return LogicMockObject.GetUsersInRole(roleName);
        }

        public override string[] GetAllRoles()
        {
            return LogicMockObject.GetAllRoles();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return LogicMockObject.FindUsersInRole(roleName, usernameToMatch);
        }

        public override string ApplicationName
        {
            get { return LogicMockObject.ApplicationName; }
            set { LogicMockObject.ApplicationName = value; }
        }
    }
}
