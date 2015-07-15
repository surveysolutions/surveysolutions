using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.Synchronization.InterviewsSynchronizerTests.Pull
{
    internal class when_pulling_entity_with_unknown_type : InterviewsSynchronizerTestsContext
    {
        private Establish context = () =>
        {
            plainStorageMock.Setup(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<LocalInterviewFeedEntry>, List<LocalInterviewFeedEntry>>>())).
                Returns(
                    new[]
                    {
                        new LocalInterviewFeedEntry()
                        {
                            EntryId = eventId.FormatGuid()
                        }
                    }.ToList());


            interviewsSynchronizer = Create.InterviewsSynchronizer(plainStorage: plainStorageMock.Object, logger: loggerMock.Object, commandService: commandServiceMock.Object);
        };

        Because of = () =>
            interviewsSynchronizer.PullInterviewsForSupervisors(new[] { Guid.NewGuid() });


        It should_write_warning_to_log_file = () =>
            loggerMock.Verify(x => x.Warn("Unknown event of type 0 received in interviews feed. It was skipped and marked as processed with error. EventId: dddddddddddddddddddddddddddddddd", null), Times.Once);

        It should_not_excecute_Any_command = () =>
            commandServiceMock.Verify(x => x.Execute(Moq.It.IsAny<ICommand>(), Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()), Times.Never);


        private static InterviewsSynchronizer interviewsSynchronizer;
        private static Guid eventId = Guid.Parse("dddddddddddddddddddddddddddddddd");

        private static Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>> plainStorageMock = new Mock<IPlainStorageAccessor<LocalInterviewFeedEntry>>();
        private static Mock<ILogger> loggerMock = new Mock<ILogger>();
        private static Mock<ICommandService> commandServiceMock = new Mock<ICommandService>();
    }
}
