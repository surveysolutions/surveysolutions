using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Domain.Exceptions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire.Completed;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Spec;

namespace Main.Core.Tests.Domain.CompleteQuestionnaire
{
    [TestFixture]
    public class SetAnswerTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test]
        public void SetAnswer_When_string_answer_passed_to_numeric_question_Then_InterviewException_is_thrown()
        {
            TestQuestionForInterviewException(new NumericQuestion() { PublicKey = Guid.NewGuid() }, "string", null);
        }

        [Test]
        public void SetAnswer_When_number_answer_passed_to_numeric_question_Then_AnswerSet_event_is_rised()
        {

            TestValueQuestionForAnswerSetEventRise(new NumericQuestion() { PublicKey = Guid.NewGuid() },
                                                       "2");
        }

        [Test]
        public void SetAnswer_When_string_answer_passed_to_AutoPropagate_question_Then_InterviewException_is_thrown()
        {
            TestQuestionForInterviewException(new AutoPropagateQuestion() { PublicKey = Guid.NewGuid() }, "string", null);
        }

        [Test]
        public void
            SetAnswer_When_number_answer_more_then_max_value_passed_to_AutoPropagate_question_Then_InterviewException_is_thrown
            ()
        {
            TestQuestionForInterviewException(new AutoPropagateQuestion() {PublicKey = Guid.NewGuid(), MaxValue = 3},
                                              "5", null);
        }

        [Test]
        public void SetAnswer_When_number_answer_passed_to_AutoPropagate_question_Then_AnswerSet_event_is_rised()
        {

            TestValueQuestionForAnswerSetEventRise(new AutoPropagateQuestion() { PublicKey = Guid.NewGuid(), MaxValue = 10},
                                                       "2");
        }


        [Test]
        public void SetAnswer_When_string_answer_passed_to_test_question_Then_AnswerSet_event_is_rised()
        {

            TestValueQuestionForAnswerSetEventRise(new TextQuestion { PublicKey = Guid.NewGuid() },
                                                       "text");
        }

        [Test]
        public void SetAnswer_When_string_answer_passed_to_datetime_question_Then_InterviewException_is_thrown()
        {
            TestQuestionForInterviewException(new DateTimeQuestion { PublicKey = Guid.NewGuid() }, "string",null);
        }

        [Test]
        public void SetAnswer_When_date_answer_passed_to_datetime_question_Then_AnswerSet_event_is_rised()
        {
            TestValueQuestionForAnswerSetEventRise(new DateTimeQuestion() { PublicKey = Guid.NewGuid() },
                                                       DateTime.Now.ToString());
        }

        [Test]
        public void SetAnswer_When_string_answer_passed_to_singleoption_question_Then_InterviewException_is_thrown()
        {
            TestQuestionForInterviewException(new SingleQuestion { PublicKey = Guid.NewGuid() }, "string", null);
        }

        [Test]
        public void SetAnswer_When_valid_option_answer_passed_to_singleoption_question_Then_AnswerSet_event_is_rised()
        {
            var singleOptionQuestion = new SingleQuestion() {PublicKey = Guid.NewGuid(), Answers = new List<IAnswer>()};
            var validOptionId = Guid.NewGuid();
            singleOptionQuestion.AddAnswer(new Answer() {PublicKey = validOptionId});
            singleOptionQuestion.AddAnswer(new Answer() {PublicKey = Guid.NewGuid()});
            TestOptionQuestionForAnswerSetEventRise(singleOptionQuestion, new List<Guid>() {validOptionId});
        }

        [Test]
        public void SetAnswer_When_invalidvalid_option_answer_passed_to_singleoption_question_Then_Then_InterviewException_is_thrown()
        {
            var singleOptionQuestion = new SingleQuestion() { PublicKey = Guid.NewGuid(), Answers = new List<IAnswer>() };
            TestQuestionForInterviewException(singleOptionQuestion,null, new List<Guid>() { Guid.NewGuid() });
        }


        [Test]
        public void SetAnswer_When_string_answer_passed_to_multyoption_question_Then_InterviewException_is_thrown()
        {
            TestQuestionForInterviewException(new MultyOptionsQuestion  { PublicKey = Guid.NewGuid() }, "string", null);
        }

        [Test]
        public void SetAnswer_When_valid_options_answer_passed_to_multyoption_question_Then_AnswerSet_event_is_rised()
        {
            var singleOptionQuestion = new MultyOptionsQuestion() { PublicKey = Guid.NewGuid(), Answers = new List<IAnswer>() };
            var validOptionId1 = Guid.NewGuid();
            var validOptionId2 = Guid.NewGuid();
            singleOptionQuestion.AddAnswer(new Answer() { PublicKey = validOptionId1 });
            singleOptionQuestion.AddAnswer(new Answer() { PublicKey = validOptionId2 });
            singleOptionQuestion.AddAnswer(new Answer() { PublicKey = Guid.NewGuid() });
            TestOptionQuestionForAnswerSetEventRise(singleOptionQuestion, new List<Guid>() { validOptionId1, validOptionId2 });
        }

        [Test]
        public void SetAnswer_When_invalidvalid_option_answer_passed_to_multyoption_question_Then_Then_InterviewException_is_thrown()
        {
            var singleOptionQuestion = new MultyOptionsQuestion() { PublicKey = Guid.NewGuid(), Answers = new List<IAnswer>() };
            TestQuestionForInterviewException(singleOptionQuestion, null, new List<Guid>() { Guid.NewGuid() });
        }

        protected void TestQuestionForInterviewException(IQuestion question, string answer, List<Guid>  answerKeys)
        {
            //arrange
            var document = new QuestionnaireDocument();
            document.Add(question, null, null);
            using (var eventContext = new EventContext())
            {
                CompleteQuestionnaireAR target = CreateCompleteQuestionnaireAR(document);

                //act

                Assert.Throws<InterviewException>(
                    () => target.SetAnswer(question.PublicKey, null, answer, answerKeys, DateTime.Now));

            }
        }

        public void TestValueQuestionForAnswerSetEventRise(IQuestion question, string answer)
        {
            //arrange
            var document = new QuestionnaireDocument();
            document.Add(question, null, null);
            using (var eventContext = new EventContext())
            {
                CompleteQuestionnaireAR target = CreateCompleteQuestionnaireAR(document);

                //act

                target.SetAnswer(question.PublicKey, null, answer, null, DateTime.Now);


                //assert
                Assert.That(GetSingleEvent<AnswerSet>(eventContext).QuestionPublicKey, Is.EqualTo(question.PublicKey));
                Assert.That(GetSingleEvent<AnswerSet>(eventContext).AnswerString, Is.EqualTo(answer));
            }
        }

        public void TestOptionQuestionForAnswerSetEventRise(IQuestion question, List<Guid> answerKeys)
        {
            //arrange
            var document = new QuestionnaireDocument();
            document.Add(question, null, null);
            using (var eventContext = new EventContext())
            {
                CompleteQuestionnaireAR target = CreateCompleteQuestionnaireAR(document);

                //act

                target.SetAnswer(question.PublicKey, null, null, answerKeys, DateTime.Now);


                //assert
                Assert.That(GetSingleEvent<AnswerSet>(eventContext).QuestionPublicKey, Is.EqualTo(question.PublicKey));
                Assert.That(GetSingleEvent<AnswerSet>(eventContext).AnswerKeys, Is.EqualTo(answerKeys));
            }
        }

        private CompleteQuestionnaireAR CreateCompleteQuestionnaireAR(QuestionnaireDocument template)
        {
            return new CompleteQuestionnaireAR(Guid.NewGuid(), template,
                                               new UserLight(Guid.NewGuid(), null));
        }
        public static T GetSingleEvent<T>(EventContext eventContext)
        {
            return (T)eventContext.Events.Single(e => e.Payload is T).Payload;
        }
    }
}
