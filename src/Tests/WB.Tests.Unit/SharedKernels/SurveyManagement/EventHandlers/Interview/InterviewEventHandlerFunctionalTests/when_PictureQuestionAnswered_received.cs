using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_PictureQuestionAnswered_received : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            var questionnaireRosterStructure = new Mock<QuestionnaireRosterStructure>();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure.Object);
            viewState = CreateViewWithSequenceOfInterviewData();
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new PictureQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                    answer)));

        It should_answer_on_picture_question_be_type_of_string = () =>
            GetAnswer().ShouldBeOfExactType<string>();

        It should_be_answer_equal_to_specified_answer = () =>
            GetAnswer().ShouldEqual(answer);

        private static object GetAnswer()
        {
            return viewState.Levels["#"].QuestionsSearchCahche[questionId].Answer;
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid questionId = Guid.Parse("13333333333333333333333333333333");
        private static string answer = "some answer here";
    }
}
