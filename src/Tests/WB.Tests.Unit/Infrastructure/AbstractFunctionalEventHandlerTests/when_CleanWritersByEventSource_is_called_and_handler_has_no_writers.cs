using System;
using Machine.Specifications;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
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
