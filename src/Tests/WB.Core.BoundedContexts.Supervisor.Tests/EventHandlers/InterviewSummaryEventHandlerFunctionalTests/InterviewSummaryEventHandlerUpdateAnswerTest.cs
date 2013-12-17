using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using QuestionAnswer = WB.Core.BoundedContexts.Supervisor.Views.Interview.QuestionAnswer;

namespace WB.Core.BoundedContexts.Supervisor.Tests.EventHandlers.InterviewSummaryEventHandlerFunctionalTests
{
    [TestFixture]
    internal class InterviewSummaryEventHandlerUpdateAnswerTest
    {
        [Test]
        public void
            Update_When_event_with_answer_on_featured_question_with_options_published_Then_answer_value_be_equal_selected_option_text()
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var answerText = "answer text";

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswerWithOptions()
                    {
                        Id = questionId,
                        Options = new List<QuestionOptions> { new QuestionOptions() { Value = 1, Text = answerText } }
                    });

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional();
            var updatedInterviewSummary =
                interviewSummaryEventHandler.Update(savedInterviewSummary,
                    CreatePublishableEvent(new SingleOptionQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, 1)));
            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions[questionId].Answer, Is.EqualTo(answerText));
        }

        [Test]
        public void
            Update_When_event_with_answer_on_featured_question_with_multy_options_published_Then_answer_value_be_equal_selected_options_text
            ()
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var options = new List<QuestionOptions>();
            for (int i = 0; i < 10; i++)
            {
                options.Add(new QuestionOptions() { Value = i, Text = i.ToString() });
            }

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswerWithOptions()
                    {
                        Id = questionId,
                        Options = options
                    });

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional();
            var updatedInterviewSummary =
                interviewSummaryEventHandler.Update(savedInterviewSummary,
                    CreatePublishableEvent(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                        new decimal[] { 1, 3, 8 })));
            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions[questionId].Answer, Is.EqualTo("1,3,8"));
        }

        [Test]
        [TestCase(QuestionType.Numeric, 1)]
        [TestCase(QuestionType.Numeric, 1.3)]
        [TestCase(QuestionType.Text, "answer text")]
        [TestCase(QuestionType.DateTime, "02/02/2012")]
        public void Update_When_event_with_answer_on_featured_question_published_Then_answer_value_be_equal_passed_answer(QuestionType type,
            object answer)
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswer()
                    {
                        Id = questionId
                    });

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional();

            var updatedInterviewSummary = CallUpdateMethod(interviewSummaryEventHandler, savedInterviewSummary,
                CreateQuestionAnsweredEventByQuestionType(questionId, type, answer));

            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions[questionId].Answer, Is.EqualTo(answer.ToString()));
        }

        private QuestionAnswered CreateQuestionAnsweredEventByQuestionType(Guid questionId, QuestionType type, object answer)
        {
            switch (type)
            {
                case QuestionType.DateTime:
                    return new DateTimeQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                        DateTime.Parse(answer.ToString()));
                case QuestionType.Numeric:
                    if (answer is int)
                        return new NumericIntegerQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, (int)answer);
                    return new NumericRealQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, Convert.ToDecimal(answer));
                case QuestionType.Text:
                    return new TextQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, (string)answer);
            }
            return null;
        }

        private InterviewSummary CallUpdateMethod(InterviewSummaryEventHandlerFunctional eventHandler, InterviewSummary currentState,
            QuestionAnswered updateEvent)
        {
            MethodInfo method = this.GetType().GetMethod("CreatePublishableEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo generic = method.MakeGenericMethod(updateEvent.GetType());
            var publishableEvent = generic.Invoke(this, new object[] { updateEvent });

            var eventType = typeof (IPublishedEvent<>).MakeGenericType(updateEvent.GetType());

            return
                (InterviewSummary)
                    eventHandler.GetType()
                        .GetMethod("Update", new Type[] { typeof (InterviewSummary), eventType })
                        .Invoke(eventHandler, new object[] { currentState, publishableEvent });
        }

        protected IPublishedEvent<T> CreatePublishableEvent<T>(T payload)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();
            publishableEventMock.Setup(x => x.Payload).Returns(payload);
            return publishableEventMock.Object;
        }

        protected static InterviewSummaryEventHandlerFunctional CreateInterviewSummaryEventHandlerFunctional()
        {
            var mockOfInterviewSummary = new Mock<IReadSideRepositoryWriter<InterviewSummary>>();
            return new InterviewSummaryEventHandlerFunctional(mockOfInterviewSummary.Object,
                new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>().Object,
                new Mock<IReadSideRepositoryWriter<UserDocument>>().Object);
        }

        protected static InterviewSummary CreateInterviewSummaryQuestions(params WB.Core.BoundedContexts.Supervisor.Views.Interview.QuestionAnswer[] questions)
        {
            var interviewSummary = new InterviewSummary();
            foreach (var question in questions)
            {
                interviewSummary.AnswersToFeaturedQuestions[question.Id] = question;
            }
            return interviewSummary;
        }
    }
}
