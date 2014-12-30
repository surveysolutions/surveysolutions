using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Web.Http.Controllers;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiBasicAuthAttributeTests
{
    internal class when_authorization_and_user_not_interviewer : ApiBasicAuthAttributeTestsContext
    {
        Establish context = () =>
        {
            var userViewFactoryMock = new Mock<IViewFactory<UserViewInputModel, UserView>>();
            userViewFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<UserViewInputModel>()))
                .Returns(new UserView() {Roles = new List<UserRoles>(new []{UserRoles.Supervisor})});

            attribute = Create((userName, password)=> true, userViewFactoryMock.Object);

            actionContext = CreateActionContext();
            actionContext.Request.Headers.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        };

        Because of = () => attribute.OnAuthorization(actionContext);

        It should_response_not_be_null = () =>
            actionContext.Response.ShouldNotBeNull();

        It should_be_unauthorized_response_status_code = () =>
            actionContext.Response.StatusCode.ShouldEqual(HttpStatusCode.Unauthorized);

        It should_response_reason_phrase_contains_specified_message = () =>
            new[] { "not", "interviewer" }.ShouldEachConformTo(keyword => actionContext.Response.ReasonPhrase.ToLower().Contains(keyword));

        private static ApiBasicAuthAttribute attribute;
        private static HttpActionContext actionContext;
    }
}