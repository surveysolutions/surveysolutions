using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.BoundedContexts.Designer.Tests.MembershipProviderTests
{

    internal class when_creating_user_and_account_with_membership_provider : MembershipProviderTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");

            customParametersWithUserId = new Dictionary<string, object>();
            customParametersWithUserId.Add("ProviderUserKey", validatedUserId);

            accountRepositoryMock = new Mock<IAccountRepository>();
            accountRepositoryMock.Setup(
                x =>
                    x.Create(it.IsAny<object>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>()))
                    .Returns(Mock.Of<IMembershipAccount>());

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(it.IsAny<string>(), it.IsAny<IPasswordPolicy>()) == true);
            
            var dependencyResolver =
                Mock.Of<IDependencyResolver>(x => x.GetService(typeof (IAccountRepository)) == accountRepositoryMock.Object &&
                    x.GetService(typeof (IPasswordPolicy)) == passwordPolicy &&
                    x.GetService(typeof (IPasswordStrategy)) == passwordStrategy);

            DependencyResolver.SetResolver(dependencyResolver);
            membershipProvider = CreateMembershipProvider();
        };

        Because of = () =>
            membershipProvider.CreateUserAndAccount(string.Empty, string.Empty, false, customParametersWithUserId);

        It should_pass_specified_provided_user_key_to_account_repository = () =>
            accountRepositoryMock.Verify(
                x => x.Create(
                    it.Is<Guid>(userId => userId == validatedUserId),
                    it.IsAny<string>(),
                    it.IsAny<string>(),
                    it.IsAny<string>()));

        private static MembershipProvider membershipProvider;
        private static Guid validatedUserId;
        private static Dictionary<string, object> customParametersWithUserId;
        private static Mock<IAccountRepository> accountRepositoryMock;
    }
}
