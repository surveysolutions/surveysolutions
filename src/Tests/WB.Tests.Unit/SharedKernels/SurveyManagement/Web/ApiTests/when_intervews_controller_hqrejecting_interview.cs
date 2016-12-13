using System;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using It = Machine.Specifications.It;
using System.Collections.Generic;
using System.Net;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests
{
    internal class when_intervews_controller_hqrejecting_interview : ApiTestContext
    {
        private Establish context = () =>
        {
            var interviewReferences =
                Mock.Of<IReadSideKeyValueStorage<InterviewReferences>>(
                    y => y.GetById(Moq.It.IsAny<string>()) == new InterviewReferences(interviewId, Guid.NewGuid(), 1));

            var userViewFactory =
                Mock.Of<IUserViewFactory>(
                    c =>
                        c.Load(Moq.It.IsAny<UserViewInputModel>()) ==
                        new UserView() {PublicKey = responsibleId, Roles = new HashSet<UserRoles>() {UserRoles.Operator}});

            var globalInfoProvider =
                Mock.Of<IGlobalInfoProvider>(
                    g => g.GetCurrentUser() == new UserLight() {Id = executorId });

            commandService = new Mock<ICommandService>();

            controller = CreateInterviewsController(commandService : commandService.Object, globalInfoProvider : globalInfoProvider, userViewFactory: userViewFactory, interviewReferences: interviewReferences);
        };

        Because of = () =>
        {
            httpResponseMessage = controller.HQReject(new StatusChangeApiModel() {Id = interviewId});
        };

        It should_return_OK_status_code = () =>
            httpResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_execute_AssignInterviewerCommand_with_specified_UserId = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<HqRejectInterviewCommand>(cp => cp.InterviewId == interviewId), Moq.It.IsAny<string>()));

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("22111111111111111111111111111111");
        private static Guid executorId = Guid.Parse("22111111111111111111111111111112");

        private static Mock<ICommandService> commandService;
        private static HttpResponseMessage httpResponseMessage;
        private static InterviewsController controller;
        
    }
}
