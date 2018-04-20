using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class ErrorsCountTests : InterviewSummaryDenormalizerTests
    {
        [Test]
        public void when_static_texts_are_declared_valid_should_not_count_errors_to_nevative_value()
        {
            var denormalizer = CreateDenormalizer();

            var state = denormalizer.Update(Create.Entity.InterviewSummary(),
                Create.Event.StaticTextsDeclaredValid(Create.Identity()).ToPublishedEvent());

            Assert.That(state, Has.Property(nameof(state.ErrorsCount)).EqualTo(0));
        }

        [Test]
        public void when_questins_declared_valid_should_not_put_negative_value_to_errors_count()
        {
            var denormalizer = CreateDenormalizer();

            var state = denormalizer.Update(Create.Entity.InterviewSummary(),
                Create.Event.AnswersDeclaredValid(Create.Identity()).ToPublishedEvent());

            Assert.That(state, Has.Property(nameof(state.ErrorsCount)).EqualTo(0));
        }

        [Test]
        public void when_answers_declared_valid_Should_Decrease_errors_count()
        {
            var denormalizer = CreateDenormalizer();

            var state = denormalizer.Update(Create.Entity.InterviewSummary(errorsCount: 5),
                Create.Event.AnswersDeclaredValid(Create.Identity(), Create.Identity()).ToPublishedEvent());

            Assert.That(state, Has.Property(nameof(state.ErrorsCount)).EqualTo(3));
        }

        [Test]
        public void when_static_texts_declared_valid_Should_Decrease_errors_count()
        {
            var denormalizer = CreateDenormalizer();

            var state = denormalizer.Update(Create.Entity.InterviewSummary(errorsCount: 5),
                Create.Event.StaticTextsDeclaredValid(Create.Identity(), Create.Identity()).ToPublishedEvent());

            Assert.That(state, Has.Property(nameof(state.ErrorsCount)).EqualTo(3));
        }

        [Test]
        public void when_questions_and_static_texts_declared_invalid_Should_increase_errors_count()
        {
            var denormalizer = CreateDenormalizer();

            var state = denormalizer.Update(Create.Entity.InterviewSummary(errorsCount: 5),
                Create.Event.StaticTextsDeclaredInvalid(Create.Identity(), Create.Identity()).ToPublishedEvent());
            state = denormalizer.Update(state,
                Create.Event.AnswersDeclaredInvalid(Create.Identity()).ToPublishedEvent());

            Assert.That(state, Has.Property(nameof(state.ErrorsCount)).EqualTo(8));
        }
    }
}
