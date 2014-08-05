using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_reevaluating_whole_interview_and_questionnaire_has_mandatory_gps_question_without_condition_or_validation : InterviewTestsContext
    {
        Establish context = () =>
        {
            var userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            var questionaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(geoQuestionId) == true
                && _.GetQuestionType(geoQuestionId) == QuestionType.DateTime
                && _.GetAllMandatoryQuestions() == new[] { geoQuestionId }
            );


            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(new GeoLocationQuestionAnswered(userId, geoQuestionId, new decimal[0], DateTime.Now, 0.111, 0.222, 333, 44, new DateTimeOffset(DateTime.Now)));
            
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        It should_raise_AnswersDeclaredValid_event_with_QuestionId_equal_to_geoQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == geoQuestionId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid questionnaireId = Guid.Parse("10000000000000000000000000000000");
        private static Guid geoQuestionId = Guid.Parse("20000000000000000000000000000000");
    }
}