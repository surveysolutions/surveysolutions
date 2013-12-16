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
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_mandatory_multy_option_question_with_empty_list_of_options : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            answeredQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_ 
                    => _.HasQuestion(answeredQuestionId) == true
                    && _.GetQuestionType(answeredQuestionId) == QuestionType.MultyOption
                    && _.IsQuestionMandatory(answeredQuestionId)==true
                    && _.IsQuestionInteger(answeredQuestionId) == true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);


            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerMultipleOptionsQuestion(userId, answeredQuestionId, new decimal[] { }, DateTime.Now, new decimal[0]);

        It should_not_raise_AnswerDeclaredValid_event = () =>
             eventContext.ShouldNotContainEvent<AnswerDeclaredValid>(@event
                 => @event.QuestionId == answeredQuestionId);

        It should_raise_AnswerDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredInvalid>(@event
                => @event.QuestionId == answeredQuestionId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid answeredQuestionId;
    }
}
