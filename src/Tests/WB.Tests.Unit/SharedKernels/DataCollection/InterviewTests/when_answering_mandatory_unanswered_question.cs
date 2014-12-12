﻿using System;
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

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("C#, KP-4387 Not yet anwered questions (mandatory and not mandatory)")]
    internal class when_answering_mandatory_unanswered_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            mandatoryQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.GetAllMandatoryQuestions() == new Guid[] { mandatoryQuestionId } 
                                                        && _.HasQuestion(mandatoryQuestionId) ==true
                                                        && _.GetQuestionType(mandatoryQuestionId)== QuestionType.Numeric
                                                        );

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
            interview.AnswerNumericRealQuestion(userId, mandatoryQuestionId, new decimal[] { }, DateTime.Now, 0);

        It should_raise_AnswersDeclaredValid_event_with_QuestionId_equal_to_mandatoryQuestionId = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredValid>(@event
                => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        It should_not_raise_AnswesrDeclaredInvalid_event_with_QuestionId_equal_to_mandatoryQuestionId = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredInvalid>(@event
                => @event.Questions.Any(question => question.Id == mandatoryQuestionId));

        private static EventContext eventContext;
        private static Guid mandatoryQuestionId;
        private static Interview interview;
        private static Guid userId;
    }
}
