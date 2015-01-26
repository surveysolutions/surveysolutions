using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_removing_flag_from_question : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            viewData = CreateViewWithSequenceOfInterviewData();
            eventHandler = CreateInterviewEventHandlerFunctional();

            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Guid.Parse("11111111111111111111111111111111");

            viewData = eventHandler.Update(viewData, CreatePublishableEvent(new FlagSetToAnswer(userId, questionId, Empty.RosterVector)));
        };

        Because of = () => viewData = eventHandler.Update(viewData, CreatePublishableEvent(new FlagRemovedFromAnswer(userId, questionId, Empty.RosterVector)));

        It should_remove_flag_from_question = () => GetQuestion(questionId, viewData).IsFlagged().ShouldBeFalse();

        static InterviewEventHandlerFunctional eventHandler;
        static InterviewData viewData;
        static Guid userId;
        static Guid questionId;
    }
}

