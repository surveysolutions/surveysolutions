using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using MembershipProvider = WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.MembershipProvider;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MembershipProviderTests
{
    internal class when_updating_user_with_email_that_used_by_another_user_with_membership_provider : MembershipProviderTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            validatedUserEmail = "some@e.mail";
            Guid userIdWithExistingEmail = Guid.Parse("11111111111111111111111111111111");

            accountRepository =
                Mock.Of<IAccountRepository>(
                    x =>
                        x.Create(it.IsAny<object>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>()) == Mock.Of<IMembershipAccount>() &&
                        x.GetUserNameByEmail(validatedUserEmail) == "userWithExistingEmail" &&
                        x.GetByProviderKey(userIdWithExistingEmail) == Mock.Of<IMembershipAccount>() &&
                        x.IsUniqueEmailRequired == true);

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(it.IsAny<string>(), it.IsAny<IPasswordPolicy>()) == true);

            Setup.InstanceToMockedServiceLocator<IAccountRepository>(accountRepository);
            Setup.InstanceToMockedServiceLocator<IPasswordPolicy>(passwordPolicy);
            Setup.InstanceToMockedServiceLocator<IPasswordStrategy>(passwordStrategy);

            membershipProvider = CreateMembershipProvider();

            membershipUser = Mock.Of<DesignerMembershipUser>(x => x.Email == validatedUserEmail && (Guid)x.ProviderUserKey == userIdWithExistingEmail);
            BecauseOf();
        }

        private void BecauseOf() => exception =
            Assert.Throws<System.Configuration.Provider.ProviderException>(() => membershipProvider.UpdateUser(membershipUser));

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__e_mail__ () =>
            exception.Message.ToLower().Should().Contain("e-mail");

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting_validated_user_email () =>
            exception.Message.Should().Contain(validatedUserEmail);

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__exists__ () =>
            exception.Message.ToLower().Should().Contain("exists");

        private static DesignerMembershipUser membershipUser;
        private static MembershipProvider membershipProvider;
        private static string validatedUserEmail;
        private static IAccountRepository accountRepository;
        private static Exception exception;
    }
}
