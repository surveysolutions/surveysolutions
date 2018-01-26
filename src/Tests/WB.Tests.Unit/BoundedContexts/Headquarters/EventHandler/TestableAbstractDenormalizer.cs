using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.EventHandler
{
    internal class TestableAbstractDenormalizer : AbstractDenormalizer<TestState>
    {
        private readonly IReadSideRepositoryWriter<InterviewEntity> testWriter;

        public TestableAbstractDenormalizer(IReadSideRepositoryWriter<InterviewEntity> testWriter)
        {
            this.testWriter = testWriter;
        }

        protected override void SaveState(TestState state)
        {
            testWriter.Store(new InterviewEntity(), "test");
        }
    }
}