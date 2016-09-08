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
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_supervisor_integer_question_and_interviewer_received_interview : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            questionId = Guid.Parse("22222222222222222222222222222222");


            var questionnaire = Mock.Of<IQuestionnaire>(_
                                                        => _.HasQuestion(questionId) == true
                                                        && _.GetQuestionType(questionId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionId) == true
                                                        && _.GetQuestionScope(questionId) == QuestionScope.Supervisor);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(new InterviewReceivedByInterviewer());
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Exception(() => interview.AnswerNumericIntegerQuestion(userId, questionId, new decimal[] { }, DateTime.Now, 0));

        It should_raise_InterviewException = () =>
        {
            exception.ShouldNotBeNull();
            exception.ShouldBeOfExactType<InterviewException>();
            exception.Message.ShouldEqual($"Can't modify Interview {interview.EventSourceId.FormatGuid()} on server, because it received by interviewer.");
        };

        It should_not_raise_any_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldNotContainEvent<NumericIntegerQuestionAnswered>();

        It should_not_raise_any_events = () =>
            eventContext.Events.ShouldBeEmpty();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static Exception exception;
    }
}
