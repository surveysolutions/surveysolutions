using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_no_writers : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWithoutWriters();
        };
        Because of = () =>
            exception = Catch.Exception(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId)) as InvalidOperationException;

        It should_throw_InvalidOperationException = () =>
            exception.ShouldNotBeNull();

        private static TestableFunctionalEventHandlerWithoutWriters testableFunctionalEventHandler;
        private static Guid eventSourceId;
        private static InvalidOperationException exception;

        internal class TestableFunctionalEventHandlerWithoutWriters : AbstractFunctionalEventHandler<IReadSideRepositoryEntity>
        {
            public TestableFunctionalEventHandlerWithoutWriters()
                : base(null) { }

            public override object[] Writers
            {
                get { return new object[0]; }
            }
        }
    }
}
