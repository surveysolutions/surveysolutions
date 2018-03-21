using System;
using NUnit.Framework;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class when_CleanWritersByEventSource_is_called_and_handler_has_writer_which_doesnt_support_cleaning : AbstractFunctionalEventHandlerTestContext
    {
        [Test] public void should_throw_InvalidOperationException () {
            eventSourceId = Guid.NewGuid();
            testableFunctionalEventHandler = new TestableFunctionalEventHandlerWithUncleanableWriter();

            Assert.That(() =>
                testableFunctionalEventHandler.CleanWritersByEventSource(eventSourceId), Throws.InvalidOperationException);
        }

        private static TestableFunctionalEventHandlerWithUncleanableWriter testableFunctionalEventHandler;
        private static Guid eventSourceId;

        public class TestableFunctionalEventHandlerWithUncleanableWriter : AbstractFunctionalEventHandler<IReadSideRepositoryEntity, IReadSideRepositoryWriter<IReadSideRepositoryEntity>>
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
