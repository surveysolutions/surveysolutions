using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_TextListQuestionAnswered_recived : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            answer = new [] { new Tuple<decimal, string>(1, "hello1"), new Tuple<decimal, string>(2, "hello2") };
            textListQuestionId = Guid.Parse("13333333333333333333333333333333");
            viewState = CreateViewWithSequenceOfInterviewData();

            var questionnaireRosterStructure = new Mock<QuestionnaireRosterStructure>();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure.Object);

            viewState = CreateViewWithSequenceOfInterviewData();
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new TextListQuestionAnswered(Guid.NewGuid(), textListQuestionId, new decimal[0], DateTime.Now,
                    answer)));

        It should_answer_on_text_list_be_type_of_InterviewTextListAnswers = () =>
            GetTextListAnswers().ShouldBeOfExactType<InterviewTextListAnswers>();

        It should_answer_on_text_list_have_2_options = () =>
           GetTextListAnswers().Answers.Length.ShouldEqual(2);

        It should_answer_on_text_list_first_option_answer_be_equal_to_first_passed_option = () =>
           GetTextListAnswers().Answers[0].Answer.ShouldEqual(answer[0].Item2);

        It should_answer_on_text_list_first_option_value_be_equal_to_first_passed_option = () =>
           GetTextListAnswers().Answers[0].Value.ShouldEqual(answer[0].Item1);

        private static InterviewTextListAnswers GetTextListAnswers()
        {
            return ((InterviewTextListAnswers)viewState.Document.Levels["#"].GetAllQuestions().First(q => q.Id == textListQuestionId).Answer);
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static ViewWithSequence<InterviewData> viewState;
        private static Guid textListQuestionId;
        private static Tuple<decimal, string>[] answer;
    }
}
