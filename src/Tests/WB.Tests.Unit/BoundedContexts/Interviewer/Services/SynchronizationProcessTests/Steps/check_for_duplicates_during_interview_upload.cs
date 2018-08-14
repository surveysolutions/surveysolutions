using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(UploadInterviews))]
    public class should_check_for_duplicates_during_interview_upload
    {
        private ISynchronizationStep step;
        private Mock<ISynchronizationService> syncService;
        private readonly Guid interviewId = Id.g1;

        [SetUp]
        public void Setup()
        {
            var interviewStorage = new InMemoryPlainStorage<InterviewView>();

            interviewStorage.Store(new InterviewView
            {
                Id = interviewId.ToString(),
                Status = InterviewStatus.Completed,
                InterviewId = interviewId
            });

            var interviewFactory = Mock.Of<IInterviewerInterviewAccessor>(iia => 
                iia.GetInteviewEventsPackageOrNull(interviewId) == new InterviewPackageApiView());

            syncService = new Mock<ISynchronizationService>();

            step = WB.Tests.Abc.Create.Service.InterviewerUploadInterviews(
                interviewFactory: interviewFactory,
                interviewViewRepository: interviewStorage,
                audioFileStorage: Mock.Of<IAudioFileStorage>(afs => afs.GetBinaryFilesForInterview(interviewId) == new List<InterviewBinaryDataDescriptor>()),
                synchronizationService: syncService.Object);
        }

        [Test]
        public async Task should_not_upload_interviews_when_there_interview_exists()
        {
            syncService
                .Setup(ss => ss.IsInterviewExists(interviewId, It.IsAny<DuplicatePackageCheck>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            
            await step.ExecuteAsync();

            syncService.Verify(ss => ss.UploadInterviewAsync(
                interviewId, 
                It.IsAny<InterviewPackageApiView>(), 
                It.IsAny<IProgress<TransferProgress>>(), 
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task should_upload_interviews_when_there_is_no_interview_exists()
        {
            syncService
                .Setup(ss =>
                    ss.IsInterviewExists(interviewId, It.IsAny<DuplicatePackageCheck>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            
            await step.ExecuteAsync();

            syncService.Verify(ss => ss.UploadInterviewAsync(
                interviewId, 
                It.IsAny<InterviewPackageApiView>(), 
                It.IsAny<IProgress<TransferProgress>>(), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
