using System.Linq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestFixture]
    [TestOf(typeof(InterviewUniqueKeyGenerator))]
    public class InterviewUniqueKeyGeneratorTests
    {
        [Test]
        public void when_unique_id_requested_should_return_it()
        {
            InterviewUniqueKeyGenerator generator = this.GetGenerator();

            var interviewKey = generator.Get();

            Assert.That(interviewKey, Is.Not.Null);
            Assert.That(interviewKey.ToString(), Is.Not.Null);
            Assert.That(interviewKey.Key, Is.Not.EqualTo(0));
        }

        [Test]
        public void when_there_is_an_interview_with_same_key_Should_pick_next_unique()
        {
            var randomSource = Create.Service.RandomValuesSource(1, 2);
            var summaries = new TestInMemoryWriter<InterviewSummary>();
            var existingInterview = Create.Entity.InterviewSummary(key: Create.Entity.InterviewKey(1).ToString());
            summaries.Store(existingInterview, "1");

            InterviewUniqueKeyGenerator generator = this.GetGenerator(summaries, randomSource);

            var interviewKey = generator.Get();

            Assert.That(interviewKey.Key, Is.EqualTo(2));
        }

        [Test]
        public void when_there_are_more_than_iterations_Should_retry_creation()
        {
            var randomSource = Create.Service.RandomValuesSource(Enumerable.Range(1, 31).ToArray());

            var summaries = new TestInMemoryWriter<InterviewSummary>();
            for (int i = 1; i < 31; i++)
            {
                var existingInterview = Create.Entity.InterviewSummary(key: Create.Entity.InterviewKey(i).ToString());
                summaries.Store(existingInterview, i.ToString());
            }

            InterviewUniqueKeyGenerator generator = this.GetGenerator(summaries, randomSource);

            var interviewKey = generator.Get();

            Assert.That(interviewKey.Key, Is.EqualTo(31));
        }

        private InterviewUniqueKeyGenerator GetGenerator(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries = null,
            IRandomValuesSource randomSource = null)
        {
            return new InterviewUniqueKeyGenerator(summaries ?? new TestInMemoryWriter<InterviewSummary>(),
                randomSource ?? Create.Service.RandomValuesSource());
        }
    }
}