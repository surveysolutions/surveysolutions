using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MembershipProviderTests
{
    [TestOf(typeof(MembershipProviderTestsContext))]
    internal class MembershipProviderTests : MembershipProviderTestsContext
    {
        [Test]
        public void when_updating_user_with_email_that_used_by_another_user_with_membership_provider()
        {
            var validatedUserEmail = "some@e.mail";
            Guid userIdWithExistingEmail = Guid.Parse("11111111111111111111111111111111");

            var accountRepository = Mock.Of<IAccountRepository>(
                x =>
                    x.Create(it.IsAny<object>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>()) == Mock.Of<IMembershipAccount>() &&
                    x.GetByNameOrEmail(validatedUserEmail) == Mock.Of<IMembershipAccount>(_=>_.UserName == "userWithExistingEmail") &&
                    x.GetByProviderKey(userIdWithExistingEmail) == Mock.Of<IMembershipAccount>() &&
                    x.IsUniqueEmailRequired == true);

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(it.IsAny<string>(), it.IsAny<IPasswordPolicy>()) == true);

            Setup.InstanceToMockedServiceLocator<IAccountRepository>(accountRepository);
            Setup.InstanceToMockedServiceLocator<IPasswordPolicy>(passwordPolicy);
            Setup.InstanceToMockedServiceLocator<IPasswordStrategy>(passwordStrategy);

            var membershipProvider = CreateMembershipProvider();

            var membershipUser = Mock.Of<DesignerMembershipUser>(x => x.Email == validatedUserEmail && (Guid)x.ProviderUserKey == userIdWithExistingEmail);

            // act
            Exception exception = Assert.Throws<System.Configuration.Provider.ProviderException>(() => membershipProvider.UpdateUser(membershipUser));

            // Assert
            exception.Message.ToLower().Should().Contain("e-mail");
            exception.Message.Should().Contain(validatedUserEmail);
            exception.Message.ToLower().Should().Contain("exists");
        }
    }
}
