using System;
using System.IO;
using System.Threading;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Views;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    internal class when_processing_package : InterviewPackagesServiceTestsContext
    {
        Establish context = () =>
        {
            var serializer =
                Mock.Of<ISerializer>(x => x.Deserialize<SyncItem>(Moq.It.IsAny<string>()) == new SyncItem() &&
                                          x.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()) == new InterviewMetaInfo { Status = 0 } &&
                                          x.Deserialize<AggregateRootEvent[]>(decompressedEvents) == new AggregateRootEvent[0]);
            var archiver = Mock.Of<IArchiveUtils>(x => x.IsZipStream(Moq.It.IsAny<Stream>()) == true &&
                                                       x.DecompressString(Moq.It.IsAny<string>()) == decompressedEvents);
            
            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();
            packagesStorage = new TestPlainStorage<InterviewPackage>();

            
            mockOfCommandService = new Mock<ICommandService>();


            interviewPackagesService = CreateInterviewPackagesService(
                archiver: archiver, serializer: serializer, brokenInterviewPackageStorage: brokenPackagesStorage,
                interviewPackageStorage: packagesStorage, commandService: mockOfCommandService.Object);

            interviewPackagesService.StorePackage(Guid.Parse("11111111111111111111111111111111"),
                Guid.Parse("22222222222222222222222222222222"), 111, Guid.Parse("33333333333333333333333333333333"),
                InterviewStatus.Restarted, false, "compressed serialized events");
        };

        Because of = () => interviewPackagesService.ProcessPackage("package id as string");

        private It should_broken_packages_storage_contains_1000_elements =
            () => mockOfCommandService.Verify(x => x.Execute(Moq.It.IsAny<SynchronizeInterviewEventsCommand>(), Moq.It.IsAny<string>()), Times.Once);

        private static Mock<ICommandService> mockOfCommandService;
        private static string decompressedEvents = "decompressed events";
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}