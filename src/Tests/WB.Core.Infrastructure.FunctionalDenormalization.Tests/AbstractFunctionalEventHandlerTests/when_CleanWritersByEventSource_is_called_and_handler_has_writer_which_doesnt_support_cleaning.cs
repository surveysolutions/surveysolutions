using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization.Tests.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_writer_which_doesnt_support_cleaning : AbstractFunctionalEventHandlerTestContext
    {
        Establish context = () =>
        {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWithUncleanableWriter();
        };
        Because of = () =>
            exception = Catch.Exception(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId)) as InvalidOperationException;

        It should_throw_InvalidOperationException = () =>
            exception.ShouldNotBeNull();

        private static TestableFunctionalEventHandlerWithUncleanableWriter testableFunctionalEventHandler;
        private static Guid eventSourceId;
        private static InvalidOperationException exception;

        internal class TestableFunctionalEventHandlerWithUncleanableWriter : AbstractFunctionalEventHandler<IReadSideRepositoryEntity>
        {
            public TestableFunctionalEventHandlerWithUncleanableWriter()
                : base(null) { }

            public override object[] Writers
            {
                get { return new object[] { new object(), }; }
            }
        }
    }
}
