using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.DenormalizerStorage;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_hqunapproving_interview : ApiTestContext
    {
        private Establish context = () =>
        {
            var interviewReferences = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviewReferences.Store(Create.Entity.InterviewSummary(interviewId, Guid.NewGuid(), questionnaireVersion: 1), "1");

            var userViewFactory =
                Mock.Of<IUserViewFactory>(
                    c =>
                        c.GetUser(Moq.It.IsAny<UserViewInputModel>()) ==
                        new UserView() {PublicKey = responsibleId, Roles = new HashSet<UserRoles>() {UserRoles.Interviewer}});

            commandService = new Mock<ICommandService>();

            controller = CreateInterviewsController(interviewReferences: interviewReferences, commandService : commandService.Object, userViewFactory: userViewFactory);
        };

        Because of = () =>
        {
            httpResponseMessage = controller.HQUnapprove(new StatusChangeApiModel() {Id = interviewId});
        };

        It should_return_OK_status_code = () =>
            httpResponseMessage.StatusCode.ShouldEqual(HttpStatusCode.OK);

        It should_execute_AssignInterviewerCommand_with_specified_UserId = () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<UnapproveByHeadquartersCommand>(cp => cp.InterviewId == interviewId), Moq.It.IsAny<string>()));

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("22111111111111111111111111111111");
        private static Guid executorId = Guid.Parse("22111111111111111111111111111112");

        private static Mock<ICommandService> commandService;
        private static HttpResponseMessage httpResponseMessage;
        private static InterviewsController controller;
        
    }
}
