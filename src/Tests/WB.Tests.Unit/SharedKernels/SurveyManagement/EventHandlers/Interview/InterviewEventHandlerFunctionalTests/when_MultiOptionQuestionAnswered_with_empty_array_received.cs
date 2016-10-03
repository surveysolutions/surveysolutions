using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_MultiOptionQuestionAnswered_with_empty_array_received : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional();
            viewState = CreateViewWithSequenceOfInterviewData();
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, answer)));

        It should_answer_on_multi_option_question_be_type_of_decimal_array = () =>
            GetAnswer().ShouldBeOfExactType<decimal[]>();

        It should_be_answer_equal_to_specified_answer = () =>
            GetAnswer().ShouldEqual(answer);

        It should_be_unanswered = () =>
            IsAnswered().ShouldBeFalse();

        It should_not_mark_question_as_flagged = () => 
            viewState.Levels["#"].QuestionsSearchCache[questionId].IsFlagged().ShouldBeFalse();

        private static object GetAnswer()
        {
            return viewState.Levels["#"].QuestionsSearchCache[questionId].Answer;
        }

        private static bool IsAnswered()
        {
            return viewState.Levels["#"].QuestionsSearchCache[questionId].IsAnswered();
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid questionId = Guid.Parse("13333333333333333333333333333333");
        private static decimal[] answer = new decimal[0];
    }
}
