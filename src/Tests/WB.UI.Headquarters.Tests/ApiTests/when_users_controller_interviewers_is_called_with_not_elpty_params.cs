using System;
using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;
using WB.UI.Headquarters.API;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ApiTests
{
    internal class when_users_controller_interviewers_is_called_with_not_elpty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            interviewersFactoryMock = new Mock<IViewFactory<InterviewersInputModel, InterviewersView>>();

            controller = CreateUsersController(interviewersViewViewFactory: interviewersFactoryMock.Object);
        };

        Because of = () =>
        {
            actionResult = controller.Intervievers(supervisorId, 10, 1);
        };

        It should_return_UserApiView = () =>
            actionResult.ShouldBeOfExactType<UserApiView>();

        It should_call_factory_load_once = () =>
            interviewersFactoryMock.Verify(x => x.Load(Moq.It.IsAny<InterviewersInputModel>()), Times.Once());
        
        private static Guid supervisorId = Guid.Parse("11111111111111111111111111111111");
        private static UserApiView actionResult;
        private static UsersController controller;
        private static Mock<IViewFactory<InterviewersInputModel, InterviewersView>> interviewersFactoryMock;
    }
}
