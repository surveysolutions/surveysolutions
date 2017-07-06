using System.Configuration.Provider;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.RoleProviderTests
{
    internal class when_adding_roles_to_users_with_non_Guid_userId : RoleProviderTestsContext
    {
        [Test]
        public void should_throw_provider_exception()
        {
            string nonGuidAccountId = "not guid user id";
            string roleName = SimpleRoleEnum.User.ToString();

            var roleRepository = Mock.Of<IRoleRepository>(x => x.Exists(null, roleName) == true);
            Setup.InstanceToMockedServiceLocator<IRoleRepository>(roleRepository);
            RoleProvider roleProvider = CreateRoleProvider();
            
            // Act
            var exception = Assert.Throws<ProviderException>(() => roleProvider.AddUsersToRoles(new[] {nonGuidAccountId}, new[] {roleName}));

            // Assert
            Assert.That(exception.Message, Does.Contain("parse"));
            Assert.That(exception.Message, Does.Contain(nonGuidAccountId));
        }
    }
}
