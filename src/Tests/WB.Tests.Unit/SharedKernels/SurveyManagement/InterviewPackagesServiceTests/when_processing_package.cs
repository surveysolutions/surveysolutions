using System;
using Main.Core.Events;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Implementation.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewPackagesServiceTests
{
    [TestFixture]
    public class when_processing_package
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var serializer =
                Mock.Of<IJsonAllTypesSerializer>(x => x.Deserialize<SyncItem>(It.IsAny<string>()) == new SyncItem() &&
                                          x.Deserialize<InterviewMetaInfo>(It.IsAny<string>()) == new InterviewMetaInfo { Status = 0 } &&
                                          x.Deserialize<AggregateRootEvent[]>(decompressedEvents) == new AggregateRootEvent[0]);
            var syncSettings = Mock.Of<SyncSettings>(x => x.UseBackgroundJobForProcessingPackages == true);

            brokenPackagesStorage = new TestPlainStorage<BrokenInterviewPackage>();
            packagesStorage = new TestPlainStorage<InterviewPackage>();
            
            mockOfCommandService = new Mock<ICommandService>();

            interviewPackagesService = Create.Service.InterviewPackagesService(
                serializer: serializer, brokenInterviewPackageStorage: brokenPackagesStorage,
                interviewPackageStorage: packagesStorage, commandService: mockOfCommandService.Object,
                syncSettings: syncSettings);
            

            interviewPackagesService.StoreOrProcessPackage(new InterviewPackage
            {
                InterviewId = Guid.Parse("11111111111111111111111111111111"),
                QuestionnaireId = Guid.Parse("22222222222222222222222222222222"),
                QuestionnaireVersion = 111,
                ResponsibleId = Guid.Parse("33333333333333333333333333333333"),
                InterviewStatus = InterviewStatus.Restarted,
                IsCensusInterview = false,
                Events = "compressed serialized events"
            });

            interviewPackagesService.ProcessPackage("1");
        }
        
        [Test]
        public void should_execute_SynchronizeInterviewEventsCommand_command() => 
            mockOfCommandService.Verify(x => x.Execute(It.IsAny<SynchronizeInterviewEventsCommand>(), It.IsAny<string>()), Times.Once);

        private static Mock<ICommandService> mockOfCommandService;
        private static string decompressedEvents = "decompressed events";
        private static InterviewPackagesService interviewPackagesService;
        private static IPlainStorageAccessor<BrokenInterviewPackage> brokenPackagesStorage;
        private static IPlainStorageAccessor<InterviewPackage> packagesStorage;
    }
}