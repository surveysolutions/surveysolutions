using System;
using Machine.Specifications;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_more_then_one_writer : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWith2Writers();
        };
        Because of = () =>
            exception = Catch.Exception(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId)) as InvalidOperationException;

        It should_throw_InvalidOperationException = () =>
            exception.ShouldNotBeNull();

        private static TestableFunctionalEventHandlerWith2Writers testableFunctionalEventHandler;
        private static Guid eventSourceId;
        private static InvalidOperationException exception;

        internal class TestableFunctionalEventHandlerWith2Writers : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>
        {
            public TestableFunctionalEventHandlerWith2Writers()
                : base(null) { }

            public override object[] Writers
            {
                get { return new object[]{new object(), new object(), }; }
            }
        }
    }
}
