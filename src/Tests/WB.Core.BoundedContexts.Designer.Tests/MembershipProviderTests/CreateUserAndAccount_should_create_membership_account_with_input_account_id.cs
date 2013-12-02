using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.MembershipProviderTests
{

    internal class CreateUserAndAccount_should_create_membership_account_with_input_account_id : MembershipProviderTestsContext
    {
        Establish context = () =>
        {
            validatedUserId = Guid.Parse("11111111111111111111111111111111");

            customParametersWithUserId = new Dictionary<string, object>();
            customParametersWithUserId.Add("ProviderUserKey", validatedUserId);

            accountRepositoryMock = new Mock<IAccountRepository>();
            accountRepositoryMock.Setup(
                x =>
                    x.Create(Moq.It.IsAny<object>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(), Moq.It.IsAny<string>()))
                    .Returns(Mock.Of<IMembershipAccount>());

            var passwordPolicy = Mock.Of<IPasswordPolicy>();
            var passwordStrategy =
                Mock.Of<IPasswordStrategy>(x => x.IsValid(Moq.It.IsAny<string>(), Moq.It.IsAny<IPasswordPolicy>()) == true);
            
            var dependencyResolver =
                Mock.Of<IDependencyResolver>(x => x.GetService(typeof (IAccountRepository)) == accountRepositoryMock.Object &&
                    x.GetService(typeof (IPasswordPolicy)) == passwordPolicy &&
                    x.GetService(typeof (IPasswordStrategy)) == passwordStrategy);

            DependencyResolver.SetResolver(dependencyResolver);
            membershipProvider = CreateMembershipProvider();
        };

        Because of = () =>
            membershipProvider.CreateUserAndAccount(string.Empty, string.Empty, false, customParametersWithUserId);

        private It should_execute_AccountRepository_Create_with_validatedUserId = () =>
            accountRepositoryMock.Verify(
                x =>
                    x.Create(Moq.It.Is<Guid>(userId => userId == validatedUserId), Moq.It.IsAny<string>(), Moq.It.IsAny<string>(),
                        Moq.It.IsAny<string>()));

        private static MembershipProvider membershipProvider;
        private static Guid validatedUserId;
        private static Dictionary<string, object> customParametersWithUserId;
        private static Mock<IAccountRepository> accountRepositoryMock;
    }
}
