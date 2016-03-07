using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2;
using WB.Core.Synchronization;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.ApiTests.InterviewerInterviewsControllerTests.v2
{
    internal class when_posting_sync_package : InterviewsApiV2ControllerTestsContext
    {
        private Establish context = () =>
        {
            mockOfSerializer.Setup(x => x.Serialize(interviewMetaInfo)).Returns(interviewMetaInfoInJsonString);
            mockOfSerializer.Setup(x => x.Serialize(Moq.It.IsAny<SyncItem>())).Returns(syncItemInJsonString);
            mockOfCompressor.Setup(x => x.CompressString(eventsInJsonString)).Returns(compressedJsonStringEvents);
            mockOfCompressor.Setup(x => x.CompressString(interviewMetaInfoInJsonString))
                .Returns(compressedJsonStringInterviewMetaInfo);

            controller = CreateInterviewerInterviewsController(
                incomingSyncPackagesQueue: mockOfIncomingSyncPackagesQueue.Object,
                serializer: mockOfSerializer.Object,
                compressor: mockOfCompressor.Object);
        };

        Because of = () => controller.Post(new InterviewPackageApiView { InterviewId = interviewId, Events = eventsInJsonString, MetaInfo = interviewMetaInfo});

        It should_add_sync_package_to_sync_packages_queue = () =>
            mockOfIncomingSyncPackagesQueue.Verify(x=>x.Enqueue(interviewId, syncItemInJsonString), Times.Once);

        It should_string_of_events_be_compressed = () =>
            mockOfCompressor.Verify(x => x.CompressString(eventsInJsonString), Times.Once);

        It should_string_of_interview_meta_info_be_compressed = () =>
            mockOfCompressor.Verify(x => x.CompressString(interviewMetaInfoInJsonString), Times.Once);

        It should_interview_meta_info_serialized_to_string = () =>
            mockOfSerializer.Verify(x => x.Serialize(interviewMetaInfo), Times.Once);

        It should_sync_package_serialized_to_string = () =>
            mockOfSerializer.Verify(x => x.Serialize(Moq.It.IsAny<SyncItem>()), Times.Once);


        private static InterviewsApiV2Controller controller;
        private static readonly Guid interviewId = Guid.Parse("11111111111111111111111111111111");
        private static readonly string compressedJsonStringEvents = "compressed events";
        private static readonly string syncItemInJsonString = "serialized sync item";
        private static readonly string eventsInJsonString = "serialized events";
        private static readonly string interviewMetaInfoInJsonString = "serialized interview meta info";
        private static readonly string compressedJsonStringInterviewMetaInfo = "compressed interview mata info";
        private static readonly InterviewMetaInfo interviewMetaInfo = new InterviewMetaInfo();
        private static readonly Mock<IIncomingSyncPackagesQueue> mockOfIncomingSyncPackagesQueue = new Mock<IIncomingSyncPackagesQueue>();
        private static readonly Mock<ISerializer> mockOfSerializer = new Mock<ISerializer>();
        private static readonly Mock<IStringCompressor> mockOfCompressor = new Mock<IStringCompressor>();
    }
}
