using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.EventHandler
{
    [TestOf(typeof(InterviewGeoLocationAnswersDenormalizer))]
    internal class InterviewGeoLocationAnswersDenormalizerTests
    {
        [Test]
        public void when_double_update_by_geo_question_answered_event_should_second_one_not_throw_an_exception()
        {
            // arrange
            var @event = Create.PublishedEvent.GeoLocationQuestionAnswered();

            var denormalizer = InterviewGeoLocationAnswersDenormalizer();

            var interviewSummary = Create.Entity.InterviewSummary();
            denormalizer.Update(interviewSummary, @event);
            // act 
            // assert
            Assert.DoesNotThrow(() => denormalizer.Update(interviewSummary, @event));
        }

        [Test]
        public void when_update_by_geo_question_answered_event_with_zero_timestamp_should_not_throw_an_exception()
        {
            // arrange
            var @event = Create.PublishedEvent.GeoLocationQuestionAnswered(timestamp: DateTimeOffset.MinValue);

            var denormalizer = InterviewGeoLocationAnswersDenormalizer();

            var interviewSummary = Create.Entity.InterviewSummary();
            denormalizer.Update(interviewSummary, @event);
            // act 
            // assert
            Assert.DoesNotThrow(() => denormalizer.Update(interviewSummary, @event));
        }

        private static InterviewGeoLocationAnswersDenormalizer InterviewGeoLocationAnswersDenormalizer(IQuestionnaireStorage questionnaireStorage = null)
            => new InterviewGeoLocationAnswersDenormalizer(questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>());
    }
}
