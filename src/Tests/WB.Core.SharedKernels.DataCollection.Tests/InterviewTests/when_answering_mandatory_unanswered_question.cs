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
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_mandatory_unanswered_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            mandatoryQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => true);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                                                                                                questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);


            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerNumericQuestion(userId, mandatoryQuestionId, new int[] { }, DateTime.Now, 0);

        It should_raise_AnswerDeclaredValid_event_with_QuestionId_equal_to_mandatoryQuestionId = () =>
            eventContext.ShouldContainEvent<AnswerDeclaredValid>(@event
              => @event.QuestionId == mandatoryQuestionId);

        It should_not_raise_AnswerDeclaredInvalid_event_with_QuestionId_equal_to_mandatoryQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswerDeclaredInvalid>(@event
             => @event.QuestionId == mandatoryQuestionId);

        private static EventContext eventContext;
        private static Guid mandatoryQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
