using System;
using NUnit.Framework;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;


namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_no_writers : AbstractFunctionalEventHandlerTestContext
    {
        [NUnit.Framework.Test] public void should_throw_InvalidOperationException () {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWithoutWriters();

            Assert.That(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId), Throws.InvalidOperationException);
        }
        
        private static TestableFunctionalEventHandlerWithoutWriters testableFunctionalEventHandler;
        private static Guid eventSourceId;

        public class TestableFunctionalEventHandlerWithoutWriters : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>
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
