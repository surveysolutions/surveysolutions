using System;
using System.Collections.Generic;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Services.Accounts;
using WB.Core.GenericSubdomains.Portable;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MembershipProviderTests
{

    internal class when_creating_user_and_account_with_membership_provider : MembershipProviderTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");

            customParametersWithUserId = new Dictionary<string, object>();
            customParametersWithUserId.Add("ProviderUserKey", validatedUserId);

            accountRepositoryMock = new Mock<IAccountRepository>();
            accountRepositoryMock.Setup(
                x =>
                    x.Create(it.IsAny<object>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>(), it.IsAny<string>()))
                    .Returns(Mock.Of<IMembershipAccount>());

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(it.IsAny<string>(), it.IsAny<IPasswordPolicy>()) == true);
            
            Setup.InstanceToMockedServiceLocator<IAccountRepository>(accountRepositoryMock.Object);
            Setup.InstanceToMockedServiceLocator<IPasswordPolicy>(passwordPolicy);
            Setup.InstanceToMockedServiceLocator<IPasswordStrategy>(passwordStrategy);

            membershipProvider = CreateMembershipProvider();
            BecauseOf();
        }

        private void BecauseOf() =>
            membershipProvider.CreateUserAndAccount(string.Empty, string.Empty, false, customParametersWithUserId);

        [NUnit.Framework.Test] public void should_pass_specified_provided_user_key_to_account_repository () =>
            accountRepositoryMock.Verify(
                x => x.Create(
                    it.Is<Guid>(userId => userId == validatedUserId),
                    it.IsAny<string>(),
                    it.IsAny<string>(),
                    it.IsAny<string>(),
                    it.IsAny<string>()));

        private static MembershipProvider membershipProvider;
        private static Guid validatedUserId;
        private static Dictionary<string, object> customParametersWithUserId;
        private static Mock<IAccountRepository> accountRepositoryMock;
    }
}
