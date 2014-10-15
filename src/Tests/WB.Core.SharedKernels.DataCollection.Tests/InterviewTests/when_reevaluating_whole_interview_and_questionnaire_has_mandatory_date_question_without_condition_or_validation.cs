using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_date_question_without_condition_or_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(dateQuestionId) == true
                && _.GetQuestionType(dateQuestionId) == QuestionType.DateTime
                && _.GetAllMandatoryQuestions() == new[] { dateQuestionId }
            );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);
            
            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new DateTimeQuestionAnswered(userId, dateQuestionId, new decimal[0], DateTime.Now, new DateTime(1985, 6, 3)));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_raise_AnswersDeclaredValid_event_with_QuestionId_equal_to_dateQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == dateQuestionId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid dateQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}