using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Utils;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_registering_supervisor_with_unique_login : SurveyTestsContext
    {
        Establish context = () =>
        {
            loginChecker = Mock.Of<ISupervisorLoginService>(x => x.IsUnique(login) == true && 
                x.AreCredentialsValid(Moq.It.IsAny<string>(), Moq.It.IsAny<string>()) == true);

            passwordHasher = Mock.Of<IPasswordHasher>(x => x.Hash(password) == passwordHash);
            var passwordPolicy = CreateApplicationPasswordPolicySettings();

            SetupInstanceToMockedServiceLocator<ISupervisorLoginService>(loginChecker);
            SetupInstanceToMockedServiceLocator<IPasswordHasher>(passwordHasher);
            SetupInstanceToMockedServiceLocator(passwordPolicy);

            survey = CreateSurvey();

            eventContext = new EventContext();
        };

        Because of = () => survey.RegisterSupervisor(login, password);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SupervisorRegistered_event = () =>
            eventContext.ShouldContainEvent<SupervisorRegistered>();

        It should_raise_SupervisorRegistered_event_with_Login_equal_to_specified_login = () =>
            eventContext.GetSingleEvent<SupervisorRegistered>().Login.ShouldEqual(login);

        It should_raise_SupervisorRegistered_event_with_Password_equal_to_specified_passwordHash = () =>
            eventContext.GetSingleEvent<SupervisorRegistered>().PasswordHash.ShouldEqual(passwordHash);

        private static EventContext eventContext;
        private static Survey survey;
        private static string login = "Vasya";
        private static string password = "VasyaLovesMeat";
        private static string passwordHash = "_A1dbbbbbbb";
        private static ISupervisorLoginService loginChecker;
        private static IPasswordHasher passwordHasher;
    }
}