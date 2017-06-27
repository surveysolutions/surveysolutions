using System;
using System.Configuration.Provider;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Roles;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.RoleProviderTests
{

    internal class when_adding_roles_to_users_with_non_Guid_userId : RoleProviderTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var roleRepository = Mock.Of<IRoleRepository>(x => x.Exists(null, roleName) == true);
            var dependencyResolver = Mock.Of<IDependencyResolver>(x => x.GetService(typeof(IRoleRepository)) == roleRepository);
            DependencyResolver.SetResolver(dependencyResolver);
            roleProvider = CreateRoleProvider();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => roleProvider.AddUsersToRoles(new[] { nonGuidAccountId }, new[] { roleName }));

        [NUnit.Framework.Test] public void should_throw_ProviderException () =>
            exception.ShouldBeOfExactType<ProviderException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing__parse__ () =>
            exception.Message.ToLower().ShouldContain("parse");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containing_not_guid_user_id () =>
            exception.Message.ShouldContain(nonGuidAccountId);    

        private static RoleProvider roleProvider;
        private static string nonGuidAccountId = "not guid user id";
        private static string roleName = SimpleRoleEnum.User.ToString();
        private static Exception exception;
    }
}
