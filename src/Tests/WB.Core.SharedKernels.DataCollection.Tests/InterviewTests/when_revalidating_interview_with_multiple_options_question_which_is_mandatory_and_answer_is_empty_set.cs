using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_revalidating_interview_with_multiple_options_question_which_is_mandatory_and_answer_is_empty_set : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
          
            var answerOptionsAsValues = new [] { 1.0m, 2.0m, 3.0m };

            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.IsQuestionMandatory(questionId) == true
                        && _.GetAllMandatoryQuestions() == new[] { questionId }
                        && _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.MultyOption
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new Guid[0] {  }
                        && _.GetAnswerOptionsAsValues(questionId) == answerOptionsAsValues
                        && _.GetAnswerOptionTitle(questionId, 1.0m) == "title 1"
                        && _.GetAnswerOptionTitle(questionId, 2.0m) == "title 2"
                        && _.GetAnswerOptionTitle(questionId, 3.0m) == "title 3"
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new MultipleOptionsQuestionAnswered(userId, questionId, emptyRosterVector, DateTime.Now, new decimal[] { 1.0m, 3.0m }));
            interview.Apply(new MultipleOptionsQuestionAnswered(userId, questionId, emptyRosterVector, DateTime.Now, new decimal[0]));
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_1_AnswersDeclaredInvalid_events = () =>
            eventContext.ShouldContainEvents<AnswersDeclaredInvalid>(count: 1);

        It should_raise_AnswersDeclaredInvalid_event_with_questionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == questionId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] emptyRosterVector;
    }
}