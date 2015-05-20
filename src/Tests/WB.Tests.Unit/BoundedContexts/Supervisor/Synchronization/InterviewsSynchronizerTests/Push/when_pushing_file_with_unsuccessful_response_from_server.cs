using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Push
{
    internal class when_pushing_file_with_unsuccessful_response_from_server : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string positiveResponse = ":)";

            fileSyncRepository.Setup(x => x.GetBinaryFilesFromSyncFolder())
                .Returns(new List<InterviewBinaryDataDescriptor>() { new InterviewBinaryDataDescriptor(interviewId, fileName, () => new byte[] { 1 }) });

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent(positiveResponse) }))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                    contentSentToHq = message.Content.ReadAsStringAsync().Result);

            var jsonUtils = Mock.Of<IJsonUtils>(utils
               => utils.Deserialize<bool>(positiveResponse) == true);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                httpMessageHandler: () => httpMessageHandler,
                interviewSynchronizationFileStorage: fileSyncRepository.Object,
                jsonUtils: jsonUtils);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_sent_file_be_not_removed_from_sync_storage = () =>
          fileSyncRepository.Verify(x => x.RemoveBinaryDataFromSyncFolder(interviewId, fileName), Times.Never);

        private static Mock<IInterviewSynchronizationFileStorage> fileSyncRepository = new Mock<IInterviewSynchronizationFileStorage>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileName = "file.jpg";
        private static string contentSentToHq;
    }
}
