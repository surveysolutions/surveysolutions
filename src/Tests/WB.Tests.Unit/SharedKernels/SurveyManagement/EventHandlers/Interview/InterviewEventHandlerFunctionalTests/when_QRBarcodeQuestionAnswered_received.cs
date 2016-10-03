using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_QRBarcodeQuestionAnswered_received : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            var questionnaireRosterStructure = new Mock<Dictionary<ValueVector<Guid>, RosterScopeDescription>>();

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure.Object);
            viewState = CreateViewWithSequenceOfInterviewData();
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new QRBarcodeQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                    answer)));

        It should_answer_on_qr_barcode_question_be_type_of_string = () =>
            GetAnswer().ShouldBeOfExactType<string>();

        It should_be_answer_equal_to_specified_answer = () =>
            GetAnswer().ShouldEqual(answer);

        private static object GetAnswer()
        {
            return viewState.Levels["#"].QuestionsSearchCache[questionId].Answer;
        }

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid questionId = Guid.Parse("13333333333333333333333333333333");
        private static string answer = "some answer here";
    }
}
