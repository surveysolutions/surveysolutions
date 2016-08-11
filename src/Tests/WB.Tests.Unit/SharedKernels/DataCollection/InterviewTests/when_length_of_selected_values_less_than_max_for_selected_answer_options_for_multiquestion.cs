using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_length_of_selected_values_less_than_max_for_selected_answer_options_for_multiquestion : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            validatingQuestionId = Guid.Parse("11111111111111111111111111111111");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.HasQuestion(validatingQuestionId) == true
                    && _.GetQuestionType(validatingQuestionId) == QuestionType.MultyOption
                    && _.GetMultiSelectAnswerOptionsAsValues(validatingQuestionId) == new decimal[] { 1, 2, 3, 4 }
                    && _.GetMaxSelectedAnswerOptions(validatingQuestionId) == 3
                );

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of =
            () => interview.AnswerMultipleOptionsQuestion(userId, validatingQuestionId, new decimal[] { }, DateTime.Now, selectedValues);

        It should_raise_MultipleOptionsQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsQuestionAnswered>(@event
                => @event.QuestionId == validatingQuestionId && @event.SelectedValues == selectedValues);

        private static Guid validatingQuestionId;
        private static decimal[] selectedValues = new decimal[] { 1, 2, 3 };
        private static Interview interview;
        private static Guid userId;
        private static EventContext eventContext;
    }
}
