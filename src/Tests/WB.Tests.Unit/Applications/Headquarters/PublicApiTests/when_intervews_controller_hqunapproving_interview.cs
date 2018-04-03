using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.DenormalizerStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_intervews_controller_hqunapproving_interview : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var interviewReferences = new InMemoryReadSideRepositoryAccessor<InterviewSummary>();
            interviewReferences.Store(Create.Entity.InterviewSummary(interviewId, Guid.NewGuid(), questionnaireVersion: 1), interviewId.FormatGuid());

            var userViewFactory =
                Mock.Of<IUserViewFactory>(
                    c =>
                        c.GetUser(Moq.It.IsAny<UserViewInputModel>()) ==
                        new UserView() {PublicKey = responsibleId, Roles = new HashSet<UserRoles>() {UserRoles.Interviewer}});

            commandService = new Mock<ICommandService>();

            controller = CreateInterviewsController(interviewReferences: interviewReferences, commandService : commandService.Object, userViewFactory: userViewFactory);
            BecauseOf();
        }

        public void BecauseOf() 
        {
            httpResponseMessage = controller.HQUnapprove(new StatusChangeApiModel() {Id = interviewId});
        }

        [NUnit.Framework.Test] public void should_return_OK_status_code () =>
            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.OK);

        [NUnit.Framework.Test] public void should_execute_AssignInterviewerCommand_with_specified_UserId () =>
            commandService.Verify(command => command.Execute(Moq.It.Is<UnapproveByHeadquartersCommand>(cp => cp.InterviewId == interviewId), Moq.It.IsAny<string>()));

        private static Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static Guid responsibleId = Guid.Parse("22111111111111111111111111111111");
        private static Guid executorId = Guid.Parse("22111111111111111111111111111112");

        private static Mock<ICommandService> commandService;
        private static HttpResponseMessage httpResponseMessage;
        private static InterviewsController controller;
        
    }
}
