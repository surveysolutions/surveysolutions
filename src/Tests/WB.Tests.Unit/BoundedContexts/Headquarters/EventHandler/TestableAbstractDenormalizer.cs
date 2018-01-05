using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.EventHandler
{
    internal class TestableAbstractDenormalizer : AbstractDenormalizer<TestState>
    {
        private readonly IReadSideRepositoryWriter<TestState> testWriter;

        public TestableAbstractDenormalizer(IReadSideRepositoryWriter<TestState> testWriter)
        {
            this.testWriter = testWriter;
        }

        protected override void SaveState(TestState state)
        {
            testWriter.Store(state, "test");
        }
    }
}