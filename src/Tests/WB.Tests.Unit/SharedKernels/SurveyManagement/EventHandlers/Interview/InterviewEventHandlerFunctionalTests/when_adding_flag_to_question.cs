using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_adding_flag_to_question : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            viewData = CreateViewWithSequenceOfInterviewData();
            eventHandler = CreateInterviewEventHandlerFunctional();

            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");
        };

        Because of = () => viewData = eventHandler.Update(viewData, CreatePublishableEvent(new FlagSetToAnswer(userId, questionId, new decimal[0])));

        It should_mark_question_as_flagged = () => GetQuestion(questionId, viewData).IsFlagged().ShouldBeTrue();

        static InterviewEventHandlerFunctional eventHandler;
        static InterviewData viewData;
        static Guid userId;
        static Guid questionId;
    }
}

