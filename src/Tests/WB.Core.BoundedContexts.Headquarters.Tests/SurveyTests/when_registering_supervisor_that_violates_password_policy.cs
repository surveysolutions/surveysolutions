using System;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.AspNet.Identity;
using NSubstitute;
using WB.Core.BoundedContexts.Headquarters.Authentication;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_registering_supervisor_that_violates_password_policy : SurveyTestsContext
    {
        Establish context = () =>
        {
            var passwordPolicyValidator = Substitute.For<CustomPasswordValidator>(0, "null");
            passwordPolicyValidator.ValidateAsync("password")
                .Returns(Task.FromResult(IdentityResult.Failed("error")));

            var supervisorLoginService = Substitute.For<ISupervisorLoginService>();
            supervisorLoginService.IsUnique("login").Returns(true);

            SetupInstanceToMockedServiceLocator(passwordPolicyValidator);
            SetupInstanceToMockedServiceLocator(supervisorLoginService);

            survey = CreateSurvey();
        };

        Because of = () => { exception = Catch.Exception(() => survey.RegisterSupervisor("login", "password")); };


        It should_thow_domain_exception_with_message_that_returned_by_password_policy = () =>
        {
            exception.ShouldBeOfExactType<SurveyException>();
            var error = exception.GetSelfOrInnerAs<SurveyException>();
            error.Message.ShouldEqual("error");
        };
        private static Survey survey;
        private static Exception exception;
    }
}