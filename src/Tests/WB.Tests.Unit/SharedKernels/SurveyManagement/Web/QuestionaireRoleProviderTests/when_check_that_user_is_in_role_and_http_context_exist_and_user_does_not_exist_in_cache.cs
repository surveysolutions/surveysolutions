using System;
using System.Collections.Generic;
using System.Web;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.QuestionaireRoleProviderTests
{
    internal class when_check_that_user_is_in_role_and_http_context_exist_and_user_does_not_exist_in_cache : QuestionnaireRoleProviderTestsContext
    {
        Establish context = () =>
        {
            var userViewFactoryMock = new Mock<IUserWebViewFactory>();
            userViewFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<UserWebViewInputModel>()))
                .Returns(new UserWebView() {Roles = new List<UserRoles>()});

            Setup.InstanceToMockedServiceLocator<IUserWebViewFactory>(userViewFactoryMock.Object);

            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://tempuri.org", null), new HttpResponse(null));
            
            provider = CreateProvider();
        };

        Because of = () => 
            exception = Catch.Exception(() => provider.IsUserInRole("some_user_name", "some_role"));

        It should_exception_be_null = () =>
            exception.ShouldBeNull();

        private static QuestionnaireRoleProvider provider;
        private static Exception exception;
    }
}
