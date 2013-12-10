using System;
using System.Web.Mvc;
using System.Web.Security;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;
using MembershipProvider = WB.UI.Shared.Web.MembershipProvider.Accounts.MembershipProvider;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.MembershipProviderTests
{

    internal class when_updating_user_with_email_that_used_by_another_user_with_membership_provider : MembershipProviderTestsContext
    {
        Establish context = () =>
        {
            validatedUserEmail = "some@e.mail";
            Guid userIdWithExistingEmail = Guid.Parse("11111111111111111111111111111111");

            accountRepository =
                Mock.Of<IAccountRepository>(
                    x =>
                        x.Create(it.IsAny<object>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>()) == Mock.Of<IMembershipAccount>() &&
                        x.GetUserNameByEmail(validatedUserEmail) == "userWithExistingEmail" &&
                        x.GetByProviderKey(userIdWithExistingEmail) == Mock.Of<IMembershipAccount>() &&
                        x.IsUniqueEmailRequired == true);

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(it.IsAny<string>(), it.IsAny<IPasswordPolicy>()) == true);
            
            var dependencyResolver =
                Mock.Of<IDependencyResolver>(x => x.GetService(typeof (IAccountRepository)) == accountRepository &&
                    x.GetService(typeof (IPasswordPolicy)) == passwordPolicy &&
                    x.GetService(typeof (IPasswordStrategy)) == passwordStrategy);

            DependencyResolver.SetResolver(dependencyResolver);
            membershipProvider = CreateMembershipProvider();

            membershipUser = Mock.Of<MembershipUser>(x => x.Email == validatedUserEmail && (Guid)x.ProviderUserKey == userIdWithExistingEmail);
        };

        private Because of = () => exception =
            Catch.Exception(() => membershipProvider.UpdateUser(membershipUser));

        It should_throw_exception_with_message_containting__e_mail__ = () =>
            exception.Message.ToLower().ShouldContain("e-mail");

        It should_throw_exception_with_message_containting_validated_user_email = () =>
            exception.Message.ShouldContain(validatedUserEmail);

        It should_throw_exception_with_message_containting__exists__ = () =>
            exception.Message.ToLower().ShouldContain("exists");

        private static MembershipUser membershipUser;
        private static MembershipProvider membershipProvider;
        private static string validatedUserEmail;
        private static IAccountRepository accountRepository;
        private static Exception exception;
    }
}
