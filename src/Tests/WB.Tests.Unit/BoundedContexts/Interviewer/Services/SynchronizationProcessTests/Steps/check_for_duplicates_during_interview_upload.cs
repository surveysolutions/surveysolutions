using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Synchronization;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests.Steps
{
    [TestOf(typeof(UploadInterviews))]
    public class should_check_for_duplicates_during_interview_upload
    {
        private Mock<ISynchronizationService> syncService;
        private readonly Guid interviewId = Id.g1;
        private Fixture fixture;

        [SetUp]
        public void Setup()
        {
            fixture = Create.Other.AutoFixture();
            fixture.Register(() => CancellationToken.None);
            var interviewStorage = new InMemoryPlainStorage<InterviewView>();
            interviewStorage.Store(new InterviewView
            {
                Id = interviewId.ToString(),
                Status = InterviewStatus.Completed,
                InterviewId = interviewId
            });

            syncService = fixture.Freeze<Mock<ISynchronizationService>>();

            fixture.Register<IPlainStorage<InterviewView>>(() => interviewStorage);
            fixture.GetMock<IInterviewerInterviewAccessor>()
                .Setup(iia => iia.GetInteviewEventsPackageOrNull(interviewId))
                .Returns(new InterviewPackageApiView());
        }

        [Test]
        public async Task should_not_upload_interviews_when_there_interview_exists()
        {
            syncService
                .Setup(ss => ss.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewUploadState
                {
                    IsEventsUploaded = true
                });

            // act
            await fixture.Create<InterviewerUploadInterviews>().ExecuteAsync();

            syncService.Verify(ss => ss.UploadInterviewAsync(
                interviewId,
                It.IsAny<InterviewPackageApiView>(),
                It.IsAny<IProgress<TransferProgress>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task should_not_upload_binaries_that_exists()
        {
            // arrange interview with 2 images and 2 audio
            fixture.GetMock<IPlainStorage<InterviewMultimediaView>>()
                .Setup(s => s.Where(It.IsAny<Expression<Func<InterviewMultimediaView, bool>>>()))
                .Returns(new List<InterviewMultimediaView>
                {
                    new InterviewMultimediaView{ FileName = "pic1.jpg", InterviewId = interviewId},
                    new InterviewMultimediaView{ FileName = "pic2.jpg", InterviewId = interviewId}
                }.AsReadOnly());

            fixture.GetMock<IAudioFileStorage>()
                .Setup(s => s.GetBinaryFilesForInterview(interviewId))
                .Returns(new List<InterviewBinaryDataDescriptor>
                {
                    Create.Entity.InterviewBinaryDataDescriptor(interviewId, "audio1.flac"),
                    Create.Entity.InterviewBinaryDataDescriptor(interviewId, "audio2.mp3")
                });

            // arrange sync service to notify that 1 audio and 1 image is already uploaded
            syncService
                .Setup(ss => ss.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewUploadState
                {
                    IsEventsUploaded = true,
                    BinaryFilesNames = new HashSet<string> { "pic1.jpg", "audio1.flac" }
                });

            await fixture.Create<InterviewerUploadInterviews>().ExecuteAsync();

            void AssertImageUploaded(string fileName, Times times) => 
                syncService.Verify(ss => ss.UploadInterviewImageAsync(interviewId, fileName, 
                        It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(),
                        It.IsAny<CancellationToken>()), times);

            AssertImageUploaded("pic1.jpg", Times.Never());
            AssertImageUploaded("pic2.jpg", Times.Once());

            void AssertAudioUploaded(string fileName, Times times) => 
                syncService.Verify(ss => ss.UploadInterviewAudioAsync(interviewId, fileName, 
                        It.IsAny<string>(),
                        It.IsAny<byte[]>(), It.IsAny<IProgress<TransferProgress>>(),
                        It.IsAny<CancellationToken>()), times);

            AssertAudioUploaded("audio1.flac", Times.Never());
            AssertAudioUploaded("audio2.mp3", Times.Once());
        }

        [Test]
        public async Task should_upload_interviews_when_there_is_no_interview_exists()
        {
            syncService
                .Setup(ss =>
                    ss.GetInterviewUploadState(interviewId, It.IsAny<EventStreamSignatureTag>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InterviewUploadState
                {
                    IsEventsUploaded = false
                });

            await fixture.Create<InterviewerUploadInterviews>().ExecuteAsync();

            syncService.Verify(ss => ss.UploadInterviewAsync(
                interviewId,
                It.IsAny<InterviewPackageApiView>(),
                It.IsAny<IProgress<TransferProgress>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
