using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.API.DataCollection.Interviewer.v2;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_posting_sync_package : InterviewsApiV2ControllerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateInterviewerInterviewsController(
                incomingSyncPackagesQueue: mockOfInterviewPackagesService.Object);
            BecauseOf();
        }

        public void BecauseOf() => controller.Post(new InterviewPackageApiView { InterviewId = interviewId, Events = eventsInJsonString, MetaInfo = interviewMetaInfo});

        [NUnit.Framework.Test] public void should_store_package_to_storage () =>
            mockOfInterviewPackagesService.Verify(x =>
                x.StoreOrProcessPackage(Moq.It.IsAny<InterviewPackage>()), Times.Once);

        private static InterviewsApiV2Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string eventsInJsonString = "serialized events";
        private static readonly InterviewMetaInfo interviewMetaInfo = new InterviewMetaInfo
        {
            TemplateId = Guid.Parse("22222222222222222222222222222222"),
            TemplateVersion = 111,
            Status = (int)InterviewStatus.Restarted,
            ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
            CreatedOnClient = true
        };
        private static readonly Mock<IInterviewPackagesService> mockOfInterviewPackagesService = new Mock<IInterviewPackagesService>();
    }
}
