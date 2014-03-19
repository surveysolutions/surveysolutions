using System.Linq;
using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Survey;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SupervisorLoginServiceTests
{
    [Subject(typeof(SupervisorLoginService))]
    public class when_existing_credentials_are_checked
    {
        Establish context = () =>
        {
            var hasher = Substitute.For<IPasswordHasher>();
            hasher.Hash("password").Returns("hash");

            credentialsStore = Substitute.For<IQueryableReadSideRepositoryReader<SupervisorCredentialsView>>();
            credentialsStore.GetById("login:hash").Returns(new SupervisorCredentialsView());

            supervisorLoginService = Create.SupervisorLoginService(credentialsStore: credentialsStore);
        };

        Because of = () => actual = supervisorLoginService.AreCredentialsValid("login", "password");

        It should_return_true = () =>  actual.ShouldBeTrue();
        private static SupervisorLoginService supervisorLoginService;
        private static IQueryableReadSideRepositoryReader<SupervisorCredentialsView> credentialsStore;
        private static bool actual;
    }
}