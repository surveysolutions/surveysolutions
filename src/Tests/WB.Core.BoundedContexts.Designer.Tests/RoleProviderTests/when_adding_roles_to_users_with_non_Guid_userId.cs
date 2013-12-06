using System;
using System.Configuration.Provider;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Roles;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.RoleProviderTests
{

    internal class when_adding_roles_to_users_with_non_Guid_userId : RoleProviderTestsContext
    {
        Establish context = () =>
        {
            var roleRepository = Mock.Of<IRoleRepository>(x => x.Exists(null, roleName) == true);
            var dependencyResolver = Mock.Of<IDependencyResolver>(x => x.GetService(typeof(IRoleRepository)) == roleRepository);
            DependencyResolver.SetResolver(dependencyResolver);
            roleProvider = CreateRoleProvider();
        };

        Because of = () =>
            exception = Catch.Exception(() => roleProvider.AddUsersToRoles(new[] { nonGuidAccountId }, new[] { roleName }));

        It should_throw_ProviderException = () =>
            exception.ShouldBeOfType<ProviderException>();

        It should_throw_exception_with_message_containing__parse__ = () =>
            exception.Message.ToLower().ShouldContain("parse");

        It should_throw_exception_with_message_containing_not_guid_user_id = () =>
            exception.Message.ShouldContain(nonGuidAccountId);    

        private static RoleProvider roleProvider;
        private static string nonGuidAccountId = "not guid user id";
        private static string roleName = SimpleRoleEnum.User.ToString();
        private static Exception exception;
    }
}
