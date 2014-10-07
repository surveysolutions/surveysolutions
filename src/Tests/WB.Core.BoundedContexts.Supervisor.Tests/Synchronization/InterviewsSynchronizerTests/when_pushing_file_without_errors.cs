using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.Views;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.InterviewsSynchronizerTests
{
    internal class when_pushing_file_without_errors : InterviewsSynchronizerTestsContext
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
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(positiveResponse) }))
                .Callback<HttpRequestMessage, CancellationToken>((message, token) =>
                    contentSentToHq = message.Content.ReadAsStringAsync().Result);

            var jsonUtils = Mock.Of<IJsonUtils>(utils
               => utils.Deserrialize<bool>(positiveResponse) == true);

            interviewsSynchronizer = Create.InterviewsSynchronizer(
                httpMessageHandler: () => httpMessageHandler,
                interviewSynchronizationFileStorage: fileSyncRepository.Object,
                jsonUtils: jsonUtils);
        };

        Because of = () =>
            interviewsSynchronizer.Push(userId);

        It should_remove_sent_file_from_sync_storage = () =>
          fileSyncRepository.Verify(x => x.RemoveBinaryDataFromSyncFolder(interviewId,fileName), Times.Once);

        private static Mock<IInterviewSynchronizationFileStorage> fileSyncRepository = new Mock<IInterviewSynchronizationFileStorage>();
        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid userId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid interviewId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileName = "file.jpg";
        private static string contentSentToHq;
    }
}
