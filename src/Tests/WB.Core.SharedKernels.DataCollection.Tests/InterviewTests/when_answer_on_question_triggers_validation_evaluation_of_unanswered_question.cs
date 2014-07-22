using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Ignore("C#")]
    internal class when_answer_on_question_triggers_validation_evaluation_of_unanswered_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            dependentOnAnsweredQuestionId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                    => _.HasQuestion(answeredQuestionId) == true
                    && _.GetQuestionType(answeredQuestionId) == QuestionType.Numeric
                    && _.IsQuestionInteger(answeredQuestionId) == true

                    && _.HasQuestion(dependentOnAnsweredQuestionId) == true
                    && _.GetQuestionType(dependentOnAnsweredQuestionId) == QuestionType.Numeric
                    && _.IsQuestionInteger(dependentOnAnsweredQuestionId) == true

                    //&& _.GetQuestionsWhichCustomValidationDependsOnSpecifiedQuestion(answeredQuestionId) == new Guid[] { dependentOnAnsweredQuestionId }
                    && _.IsCustomValidationDefined(dependentOnAnsweredQuestionId) == true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
              interview.AnswerNumericIntegerQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, 1);

        It should_raise_AnswersDeclaredValid_event = () =>
             eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event
                 => @event.Questions.Any(question => question.Id == dependentOnAnsweredQuestionId));

        It should_not_raise_AnswersDeclaredInvalid_event = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>(@event
                 => @event.Questions.Any(question => question.Id == dependentOnAnsweredQuestionId));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview; 
        private static Guid userId;
        private static Guid answeredQuestionId;
        private static Guid dependentOnAnsweredQuestionId;
    }
}
