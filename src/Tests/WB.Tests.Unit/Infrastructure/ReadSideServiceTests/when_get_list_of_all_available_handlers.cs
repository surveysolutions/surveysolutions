using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideServiceTests
{
    internal class when_get_list_of_all_available_handlers : ReadSideServiceTestContext
    {
        Establish context = () =>
        {
            readSideRepositoryCleanerMock = new Mock<IReadSideRepositoryCleaner>();
            readSideRepositoryWriterMock = new Mock<IChacheableRepositoryWriter>();
            readSideRepositoryWriterMock.Setup(x => x.ViewType).Returns(typeof(object));

            firstEventHandlerMock = new Mock<IEventHandler>();

            firstEventHandlerMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryCleanerMock.Object, readSideRepositoryWriterMock.Object });
            firstEventHandlerMock.Setup(x => x.Name).Returns("first handler");

            secondEventHandlerMock = new Mock<IEventHandler>();

            secondEventHandlerMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryCleanerMock.Object, readSideRepositoryWriterMock.Object });
            secondEventHandlerMock.Setup(x => x.Name).Returns("second handler");

            eventDispatcherMock = new Mock<IEventDispatcher>();
            eventDispatcherMock.Setup(x => x.GetAllRegistredEventHandlers()).Returns(new[] { firstEventHandlerMock.Object, secondEventHandlerMock.Object });

          
            ravenReadSideService = CreateReadSideService(eventDispatcher: eventDispatcherMock.Object);
        };

        Because of = () => result = ravenReadSideService.GetAllAvailableHandlers();

        It should_rebuild_all_view = () =>
            result.Select(x => x.Name).ToArray().ShouldEqual(new[] { FirstHandlerName, SecondHandlerName });

        private static ReadSideService ravenReadSideService;
        private static Mock<IEventDispatcher> eventDispatcherMock;
        private static Mock<IEventHandler> firstEventHandlerMock;
        private static Mock<IEventHandler> secondEventHandlerMock;
        private static Mock<IReadSideRepositoryCleaner> readSideRepositoryCleanerMock;
        private static Mock<IChacheableRepositoryWriter> readSideRepositoryWriterMock;
        private static IEnumerable<ReadSideEventHandlerDescription> result;

        private static string FirstHandlerName = "first handler";
        private static string SecondHandlerName = "second handler";
    }
}
