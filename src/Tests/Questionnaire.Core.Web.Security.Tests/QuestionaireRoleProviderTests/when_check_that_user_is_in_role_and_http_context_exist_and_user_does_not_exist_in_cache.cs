using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.User;
using Microsoft.Practices.ServiceLocation;
using Moq;
using It = Machine.Specifications.It;

namespace Questionnaire.Core.Web.Security.Tests.QuestionaireRoleProviderTests
{
    internal class when_check_that_user_is_in_role_and_http_context_exist_and_user_does_not_exist_in_cache : QuestionnaireRoleProviderTestsContext
    {
        Establish context = () =>
        {
            var userViewFactoryMock = new Mock<IViewFactory<UserViewInputModel, UserView>>();
            userViewFactoryMock.Setup(_ => _.Load(Moq.It.IsAny<UserViewInputModel>()))
                .Returns(new UserView() {Roles = new List<UserRoles>()});

            var serviceLocatorMock = new Mock<IServiceLocator> {DefaultValue = DefaultValue.Mock};
            serviceLocatorMock.Setup(_ =>_.GetInstance<IViewFactory<UserViewInputModel, UserView>>()).Returns(userViewFactoryMock.Object);
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

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
