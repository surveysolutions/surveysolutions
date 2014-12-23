using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    internal class when_registering_event_handler_from_ignore_list : NcqrCompatibleEventDispatcherTestContext
    {
        Establish context = () =>
        {
            ncqrsStyleDenormalizerMock = new Mock<IEventHandler>();

            ncqrCompatibleEventDispatcher =
                CreateNcqrCompatibleEventDispatcher(handlersToIgnore: new[] { ncqrsStyleDenormalizerMock.Object.GetType() });
        };

        Because of = () => ncqrCompatibleEventDispatcher.Register(ncqrsStyleDenormalizerMock.Object);

        It should_lits_of_register_event_handlers_be_empty = () =>
            ncqrCompatibleEventDispatcher.GetAllRegistredEventHandlers().ShouldBeEmpty();

        private static NcqrCompatibleEventDispatcher ncqrCompatibleEventDispatcher;
        private static Mock<IEventHandler> ncqrsStyleDenormalizerMock;
    }
}
