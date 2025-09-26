using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Services
{
    [TestOf(typeof(SupervisorInterviewUploadStateHandler))]
    public class SupervisorInterviewUploadStateHandlerTests
    {
        [Test]
        public async Task should_detect_duplicate_packages()
        {
            var fixture = Create.Other.AutoFixture();

            (DateTime first, DateTime last) timestamps = (DateTime.Now, DateTime.UtcNow);

            var recievedPackagesLog = Create.Storage.InMemorySqlitePlainStorage<SuperivsorReceivedPackageLogEntry, int>();
            recievedPackagesLog.Store(new SuperivsorReceivedPackageLogEntry
            {
                FirstEventId = Id.gA,
                LastEventId = Id.gF,
                FirstEventTimestamp = timestamps.first,
                LastEventTimestamp = timestamps.last
            });

            fixture.Register<IPlainStorage<SuperivsorReceivedPackageLogEntry, int>>(() => recievedPackagesLog);

            var handler = fixture.Create<SupervisorInterviewUploadStateHandler>();

            var response = await handler.GetInterviewUploadState(new GetInterviewUploadStateRequest
            {
                InterviewId = Id.g1,
                Check = new EventStreamSignatureTag
                {
                    FirstEventId = Id.gA,
                    LastEventId = Id.gF,
                    FirstEventTimeStamp = timestamps.first,
                    LastEventTimeStamp = timestamps.last
                }
            });

            Assert.That(response.UploadState.IsEventsUploaded, Is.EqualTo(true));
        }

        [Test]
        public async Task should_return_list_of_uploaded_files()
        {
            var fixture = Create.Other.AutoFixture();

            fixture.GetMock<IImageFileStorage>()
                .Setup(fs => fs.GetBinaryFilesForInterview(Id.g1))
                .ReturnsAsync(new List<InterviewBinaryDataDescriptor>
                {
                    new InterviewBinaryDataDescriptor(Id.g1, "pic1.jpg", "image/data", 
                        () => Task.FromResult(Array.Empty<byte>()), "pic1_md5")
                });

            fixture.GetMock<IAudioFileStorage>()
                .Setup(fs => fs.GetBinaryFilesForInterview(Id.g1))
                .ReturnsAsync(new List<InterviewBinaryDataDescriptor>
                {
                    new InterviewBinaryDataDescriptor(Id.g1, "audio.jpg", "audio/data", 
                        () => Task.FromResult(Array.Empty<byte>()), "audio_md5")
                });

            var handler = fixture.Create<SupervisorInterviewUploadStateHandler>();

            var response = await handler.GetInterviewUploadState(new GetInterviewUploadStateRequest
            {
                InterviewId = Id.g1,
                Check = new EventStreamSignatureTag()
            });

            Assert.That(response.UploadState.AudioFilesNames, Contains.Item("audio.jpg"));
            Assert.That(response.UploadState.ImagesFilesNames, Contains.Item("pic1.jpg"));
        }
    }
}
