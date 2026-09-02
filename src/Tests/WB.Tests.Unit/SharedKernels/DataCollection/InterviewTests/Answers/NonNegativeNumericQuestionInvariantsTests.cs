using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Answers
{
    [TestOf(typeof(Interview))]
    public class NonNegativeNumericQuestionInvariantsTests : InterviewTestsContext
    {
        readonly Guid questionId = Id.g1;
        readonly Guid userId = Id.gA;

        private StatefulInterview CreateNonNegativeIntegerInterview(IEnumerable<Main.Core.Entities.SubEntities.Answer> specialValues = null)
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId, isNonNegative: true,
                    specialValues: specialValues));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: true,
                questionnaire: questionnaire);
            return interview;
        }

        private StatefulInterview CreateNonNegativeRealInterview(IEnumerable<Main.Core.Entities.SubEntities.Answer> specialValues = null)
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(questionId, isNonNegative: true,
                    specialValues: specialValues));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: true,
                questionnaire: questionnaire);
            return interview;
        }

        [Test]
        public void when_answering_non_negative_integer_question_with_negative_value_should_throw()
        {
            var interview = CreateNonNegativeIntegerInterview();

            TestDelegate act = () => interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -5);

            Assert.That(act, Throws.TypeOf<AnswerNotAcceptedException>());
        }

        [Test]
        public void when_answering_non_negative_integer_question_with_negative_special_value_should_not_throw()
        {
            var interview = CreateNonNegativeIntegerInterview(specialValues: Create.Entity.Options(-1));

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -1));
        }

        [Test]
        public void when_answering_non_negative_integer_question_with_positive_value_should_not_throw()
        {
            var interview = CreateNonNegativeIntegerInterview();

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, 42));
        }

        [Test]
        public void when_answering_non_negative_integer_question_flag_is_false_and_negative_value_should_not_throw()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId, isNonNegative: false));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: true,
                questionnaire: questionnaire);

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -5));
        }

        [Test]
        public void when_answering_non_negative_real_question_with_negative_value_should_throw()
        {
            var interview = CreateNonNegativeRealInterview();

            TestDelegate act = () => interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -3.5);

            Assert.That(act, Throws.TypeOf<AnswerNotAcceptedException>());
        }

        [Test]
        public void when_answering_non_negative_real_question_with_negative_special_value_should_not_throw()
        {
            var interview = CreateNonNegativeRealInterview(specialValues: Create.Entity.Options(-1));

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -1.0));
        }

        [Test]
        public void when_answering_non_negative_real_question_with_positive_value_should_not_throw()
        {
            var interview = CreateNonNegativeRealInterview();

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, 3.14));
        }

        [Test]
        public void when_answering_non_negative_real_question_flag_is_false_and_negative_value_should_not_throw()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(questionId, isNonNegative: false));

            var interview = Create.AggregateRoot.StatefulInterview(shouldBeInitialized: true,
                questionnaire: questionnaire);

            Assert.DoesNotThrow(() =>
                interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -2.5));
        }

        [Test]
        public void when_answering_non_negative_real_question_with_out_of_decimal_range_negative_value_should_throw()
        {
            var interview = CreateNonNegativeRealInterview();

            TestDelegate act = () => interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, -1e100);

            Assert.That(act, Throws.TypeOf<AnswerNotAcceptedException>());
        }

        [Test]
        public void when_preloading_non_negative_integer_question_with_negative_value_should_throw()
        {
            TestDelegate act = () =>
                Create.AggregateRoot.StatefulInterview(
                    shouldBeInitialized: true,
                    questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.NumericIntegerQuestion(questionId, isNonNegative: true)),
                    answers: new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(
                            Create.Entity.Identity(questionId, RosterVector.Empty),
                            NumericIntegerAnswer.FromInt(-5))
                    });

            Assert.That(act, Throws.TypeOf<AnswerNotAcceptedException>());
        }

        [Test]
        public void when_preloading_non_negative_integer_question_with_negative_special_value_should_not_throw()
        {
            Assert.DoesNotThrow(() =>
                Create.AggregateRoot.StatefulInterview(
                    shouldBeInitialized: true,
                    questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.NumericIntegerQuestion(questionId, isNonNegative: true, specialValues: Create.Entity.Options(-1))),
                    answers: new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(
                            Create.Entity.Identity(questionId, RosterVector.Empty),
                            NumericIntegerAnswer.FromInt(-1))
                    }));
        }

        [Test]
        public void when_preloading_non_negative_real_question_with_negative_value_should_throw()
        {
            TestDelegate act = () =>
                Create.AggregateRoot.StatefulInterview(
                    shouldBeInitialized: true,
                    questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.NumericRealQuestion(questionId, isNonNegative: true)),
                    answers: new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(
                            Create.Entity.Identity(questionId, RosterVector.Empty),
                            NumericRealAnswer.FromDouble(-2.5))
                    });

            Assert.That(act, Throws.TypeOf<AnswerNotAcceptedException>());
        }

        [Test]
        public void when_preloading_non_negative_real_question_with_negative_special_value_should_not_throw()
        {
            Assert.DoesNotThrow(() =>
                Create.AggregateRoot.StatefulInterview(
                    shouldBeInitialized: true,
                    questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(
                        Create.Entity.NumericRealQuestion(questionId, isNonNegative: true, specialValues: Create.Entity.Options(-1))),
                    answers: new List<InterviewAnswer>
                    {
                        Create.Entity.InterviewAnswer(
                            Create.Entity.Identity(questionId, RosterVector.Empty),
                            NumericRealAnswer.FromDouble(-1.0))
                    }));
        }
    }
}
