using System;
using NUnit.Framework;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_more_then_one_writer : AbstractFunctionalEventHandlerTestContext
    {
        [NUnit.Framework.Test] public void should_throw_InvalidOperationException () {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWith2Writers();

            Assert.Throws<InvalidOperationException>(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId));
        }

        private static TestableFunctionalEventHandlerWith2Writers testableFunctionalEventHandler;
        private static Guid eventSourceId;

        public class TestableFunctionalEventHandlerWith2Writers : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>
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
