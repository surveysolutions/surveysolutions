using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Services;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_registering_supervisor_with_not_unique_login : SurveyTestsContext
    {
        Establish context = () =>
        {
            loginChecker = Mock.Of<ILoginsChecker>(x => x.IsUnique(login) == false);

            SetupInstanceToMockedServiceLocator<ILoginsChecker>(loginChecker);

            survey = CreateSurvey();

            eventContext = new EventContext();
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
                survey.RegisterSupervisor(login, password));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_throw_SurveyException = () =>
            exception.ShouldBeOfExactType<SurveyException>();

        It should_throw_exception_with_message_containing__name____empty__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("supervisor's", "login", "taken");

        private static EventContext eventContext;
        private static Survey survey;
        private static string login = "Vasya";
        private static string password = "VasyaLovesMeat";
        private static ILoginsChecker loginChecker;
        private static Exception exception;
    }
}