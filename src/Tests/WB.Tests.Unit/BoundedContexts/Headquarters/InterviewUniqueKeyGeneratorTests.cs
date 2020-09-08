using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ReflectionMagic;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Internal;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
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
            Assert.That(interviewKey.RawValue, Is.Not.EqualTo(0));
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

            Assert.That(interviewKey.RawValue, Is.EqualTo(2));
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

            Assert.That(interviewKey.RawValue, Is.EqualTo(31));
        }

        [Test]
        public void should_throw_on_failed_to_get_key()
        {
            var randomSource = Create.Service.RandomValuesSource(5);

            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.Entity.InterviewSummary(key: new InterviewKey(5).ToString()), "id1");

            var generator = GetGenerator(summaries, randomSource);

            // Act
            TestDelegate act = () => generator.Get();

            // Assert
            Assert.That(act, Throws.InstanceOf<InterviewUniqueKeyGeneratorException>());
        }

        [Test]
        public void when_generator_cant_find_new_human_id_Should_increase_humanId_size()
        {
            var randomSource = new Mock<IRandomValuesSource>();
            randomSource.Setup(x => x.Next(99_99_99_99))
                .Returns(5);

            const int increasedHumanId = 6;
            randomSource.Setup(x => x.Next(int.MaxValue))
             .Returns(increasedHumanId);

            var summaries = new TestInMemoryWriter<InterviewSummary>();
            summaries.Store(Create.Entity.InterviewSummary(key: new InterviewKey(5).ToString()), "id1");

            var naturalKeySettingsStorage = new TestInMemoryKeyValueStorage<NaturalKeySettings>();
            var generator = GetGenerator(summaries, randomSource.Object, naturalKeySettingsStorage);

            // Act
            var key = generator.Get();

            // Assert
            Assert.That(key, Has.Property(nameof(key.RawValue)).EqualTo(increasedHumanId));

            var storedNaturalKey = naturalKeySettingsStorage.GetById(AppSetting.NatualKeySettings);
            Assert.That(storedNaturalKey, Is.Not.Null, "Should store new max value for natural key");
        }

        private InterviewUniqueKeyGenerator GetGenerator(
            IQueryableReadSideRepositoryReader<InterviewSummary> summaries = null,
            IRandomValuesSource randomSource = null,
            IPlainKeyValueStorage<NaturalKeySettings> naturalKeySettings = null)
        {
            var interviewUniqueKeyGenerator = new InterviewUniqueKeyGenerator(summaries ?? new TestInMemoryWriter<InterviewSummary>(),
                naturalKeySettings ?? new TestInMemoryKeyValueStorage<NaturalKeySettings>(),
                randomSource ?? Create.Service.RandomValuesSource(),
                Mock.Of<ILogger<InterviewUniqueKeyGenerator>>());

            InterviewUniqueKeyGenerator.maxInterviewKeyValue = 0;
            
            return interviewUniqueKeyGenerator;
        }
    }
}
