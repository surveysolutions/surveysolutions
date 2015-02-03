using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_DeQueue_called_and_interview_were_created_on_client : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var @event = new AggregateRootEvent { EventTimeStamp = initialTimestamp };

            var jsonUtils = Mock.Of<IJsonUtils>(_ =>
                _.Deserialize<AggregateRootEvent[]>(Moq.It.IsAny<string>()) == new[] {@event}
                && _.Deserialize<SyncItem>(Moq.It.IsAny<string>()) == new SyncItem() {MetaInfo = "test"}
                &&
                _.Deserialize<InterviewMetaInfo>(Moq.It.IsAny<string>()) ==
                new InterviewMetaInfo()
                {
                    CreatedOnClient = true,
                    FeaturedQuestionsMeta = new[] {new FeaturedQuestionMeta(Guid.NewGuid(), "test", "test value")}
                });

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(_ =>
                _.GetFilesInDirectory(Moq.It.IsAny<string>()) == new[] { Guid.NewGuid().FormatGuid() }
                && _.IsFileExists(Moq.It.IsAny<string>()) == true);

            var eventStore = Mock.Of<IStreamableEventStore>();
            Mock.Get(eventStore)
                .Setup(store => store.Store(Moq.It.IsAny<UncommittedEventStream>()))
                .Callback<UncommittedEventStream>(stream => storedStream = stream);

            commandServiceMock=new Mock<ICommandService>();
            incomingPackagesQueue = CreateIncomePackagesRepository(eventStore: eventStore, fileSystemAccessor: fileSystemAccessor, jsonUtils: jsonUtils, commandService:commandServiceMock.Object,
                interviewSummaryStorage:
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(_ => _.GetById(interviewId.FormatGuid()) == new InterviewSummary()));
        };

        Because of = () =>
            incomingPackagesQueue.DeQueue();

        It should_not_change_event_timespamp = () =>
            storedStream.Single().EventTimeStamp.ShouldEqual(initialTimestamp);

        It should_CreateInterviewCreatedOnClientCommand_should_be_called = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<CreateInterviewCreatedOnClientCommand>(), Moq.It.IsAny<string>()), Times.Once);

        It should_ApplySynchronizationMetadata_should_be_never_called = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<ApplySynchronizationMetadata>(), Moq.It.IsAny<string>()), Times.Never);

        private static UncommittedEventStream storedStream;
        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static IncomingPackagesQueue incomingPackagesQueue;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Mock<ICommandService> commandServiceMock;
    }
}
