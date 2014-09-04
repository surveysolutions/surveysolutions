using System;
using System.Linq;
using Machine.Specifications;
using Main.Core;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.IncomePackagesRepositoryTests
{
    internal class when_processing_item : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            var @event = new AggregateRootEvent { EventTimeStamp = initialTimestamp };

            var jsonUtils = Mock.Of<IJsonUtils>(_
                => _.Deserrialize<AggregateRootEvent[]>(Moq.It.IsAny<string>()) == new [] { @event });

            var fileSystemAccessor = Mock.Of<IFileSystemAccessor>(_
                => _.IsFileExists(Moq.It.IsAny<string>()) == true
                && _.ReadAllText(Moq.It.IsAny<string>()) == PackageHelper.CompressString("x"));

            var eventStore = Mock.Of<IStreamableEventStore>();
            Mock.Get(eventStore)
                .Setup(store => store.Store(Moq.It.IsAny<UncommittedEventStream>()))
                .Callback<UncommittedEventStream>(stream => storedStream = stream);

            incomePackagesRepository = CreateIncomePackagesRepository(eventStore: eventStore, fileSystemAccessor: fileSystemAccessor, jsonUtils: jsonUtils);
        };

        Because of = () =>
            incomePackagesRepository.ProcessItem(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"));

        It should_not_change_event_timespamp = () =>
            storedStream.Single().EventTimeStamp.ShouldEqual(initialTimestamp);

        private static UncommittedEventStream storedStream;
        private static readonly DateTime initialTimestamp = new DateTime(2012, 04, 22);
        private static IncomePackagesRepository incomePackagesRepository;
    }
}