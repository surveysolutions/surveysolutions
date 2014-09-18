using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pushing_file_and_responce_cant_be_dematerialized : InterviewsSynchronizerTestsContext
    {
        Establish context = () =>
        {
            string positiveResponse = ":)";

            fileSyncRepository.Setup(x => x.GetBinaryFilesFromSyncFolder())
                .Returns(new List<InterviewBinaryData>() { new InterviewBinaryData(interviewId, fileName, () => new byte[] { 1 }) });

            var httpMessageHandler = Mock.Of<HttpMessageHandler>();

            Mock.Get(httpMessageHandler)
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() =>
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(positiveResponse) }))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                    contentSentToHq = message.Content.ReadAsStringAsync().Result);

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<bool>(positiveResponse)).Throws<NullReferenceException>();

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                httpMessageHandler: () => httpMessageHandler,
                fileSyncRepository: fileSyncRepository.Object,
                jsonUtils: jsonUtilsMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_sent_file_be_not_removed_from_sync_storage = () =>
          fileSyncRepository.Verify(x => x.RemoveBinaryDataFromSyncFolder(interviewId, fileName), Times.Never);

        private static Mock<IFileSyncRepository> fileSyncRepository = new Mock<IFileSyncRepository>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileName = "file.jpg";
        private static string contentSentToHq;
    }
}
