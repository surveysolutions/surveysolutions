using System;
using System.Linq;
using Machine.Specifications;
using Main.Core;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.IncomePackagesRepositoryTests
{
    internal class when_processing_item : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var @event = new AggregateRootEvent { EventTimeStamp = initialTimestamp };

            var jsonUtils = Mock.Of<IJsonUtils>(_
                => _.Deserialize<AggregateRootEvent[]>(Moq.It.IsAny<string>()) == new [] { @event });

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(_
                => _.IsFileExists(Moq.It.IsAny<string>()) == true
                && _.ReadAllText(Moq.It.IsAny<string>()) == new ZipArchiveUtils(Mock.Of<IFileSystemAccessor>()).CompressString("x"));

            var eventStore = Mock.Of<IStreamableEventStore>();
            Mock.Get(eventStore)
                .Setup(store => store.Store(Moq.It.IsAny<UncommittedEventStream>()))
                .Callback<UncommittedEventStream>(stream => storedStream = stream);

            incomePackagesRepository = CreateIncomePackagesRepository(eventStore: eventStore, fileSystemAccessor: fileSystemAccessor, jsonUtils: jsonUtils,
                interviewSummaryStorage:
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(_ => _.GetById(interviewId.FormatGuid()) == new InterviewSummary()));
        };

        Because of = () =>
            incomePackagesRepository.ProcessItem(interviewId);

        It should_not_change_event_timespamp = () =>
            storedStream.Single().EventTimeStamp.ShouldEqual(initialTimestamp);

        private static UncommittedEventStream storedStream;
        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static IncomePackagesRepository incomePackagesRepository;
        private static Guid interviewId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
} 