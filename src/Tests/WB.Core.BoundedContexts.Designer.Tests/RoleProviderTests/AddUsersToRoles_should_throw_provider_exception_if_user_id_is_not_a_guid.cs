using System;
using System.Configuration.Provider;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Roles;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.RoleProviderTests
{

    internal class AddUsersToRoles_should_throw_provider_exception_if_user_id_is_not_be_parsed_as_guid : RoleProviderTestsContext
    {
        Establish context = () =>
        {
            var roleRepository = Mock.Of<IRoleRepository>(x => x.Exists(null, roleName) == true);
            var dependencyResolver = Mock.Of<IDependencyResolver>(x => x.GetService(typeof(IRoleRepository)) == roleRepository);
            DependencyResolver.SetResolver(dependencyResolver);
            roleProvider = CreateRoleProvider();
        };

        Because of = () =>
            exception = Catch.Exception(() => roleProvider.AddUsersToRoles(new[] { validatedUserId }, new[] { roleName }));

        It should_throw_ProviderException = () =>
            exception.ShouldBeOfType<ProviderException>();

        It should_throw_exception_with_message_containting__parse__ = () =>
            exception.Message.ToLower().ShouldContain("parse");

        It should_throw_exception_with_message_containting_not_guid_user_id = () =>
            exception.Message.ShouldContain(validatedUserId);    

        private static RoleProvider roleProvider;
        private static string validatedUserId = "not guid user id";
        private static string roleName = SimpleRoleEnum.User.ToString();
        private static Exception exception;
    }
}
