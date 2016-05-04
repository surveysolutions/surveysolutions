using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.FilterTests.SupervisorFunctionsEnabledAttributeTests
{
    internal class when_action_executing_and_supervisor_functions_disabled_and_interviewer_controller : SupervisorFunctionsEnabledAttributeTestsContext
    {
        Establish context = () =>
        {
            WebConfigurationManager.AppSettings["SupervisorFunctionsEnabled"] = "false";
            filter = Create();
        };

        Because of = () => exception = Catch.Exception(()=> filter.OnActionExecuting(actionExecutingContext));

        It should_throw_http_exception_with_404_ = () =>
        {
            exception.ShouldBeOfExactType<HttpException>();
            ((HttpException) exception).GetHttpCode().ShouldEqual(404);
        };

        private static Exception exception;
        private static SupervisorFunctionsEnabledAttribute filter;
        private static readonly HttpActionContext actionExecutingContext = CreateFilterContext(new InterviewerApiV1Controller(null, null, null, null));
    }
}