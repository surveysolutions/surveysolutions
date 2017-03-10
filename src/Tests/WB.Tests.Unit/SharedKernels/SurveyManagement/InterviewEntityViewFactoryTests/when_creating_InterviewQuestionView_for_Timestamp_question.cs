using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    internal class when_creating_InterviewQuestionView_for_Timestamp_question : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionTemplate = Create.Entity.DateTimeQuestion(questionId: questionId, isTimestamp: true);
            questionData = Create.Entity.InterviewQuestion(questionId);
            questionData.Answer = new DateTime(2016, 06, 07, 12, 0, 01);
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () => timestampQuestionView = interviewEntityViewFactory.BuildInterviewQuestionView(questionTemplate, questionData, null, false, new decimal[0], InterviewStatus.Completed);

        It should_view_has_specified_settings = () =>
            ((DateTimeQuestionSettings)timestampQuestionView.Settings).IsTimestamp.ShouldEqual(true);

        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView timestampQuestionView;
        private static DateTimeQuestion questionTemplate;
        private static InterviewQuestion questionData;
        private static readonly Guid questionId = Guid.Parse("44444444444444444444444444444444");
    }
}
