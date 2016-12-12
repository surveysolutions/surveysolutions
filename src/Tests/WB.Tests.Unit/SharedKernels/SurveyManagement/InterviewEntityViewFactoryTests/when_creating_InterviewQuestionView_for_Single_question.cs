using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewEntityViewFactoryTests
{
    internal class when_creating_InterviewQuestionView_for_Single_question : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            textListQuestionTemplate = new SingleQuestion() {
                Answers = new List<Answer>() {
                    new Answer() {AnswerValue = "1", AnswerText = "1"},
                    new Answer() {AnswerValue = "2", AnswerText = "2"} } };
            textListQuestionData = new InterviewQuestion(textListQuestionId)
            {
                Answer = null
            };
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () => textListQuestionView = interviewEntityViewFactory.BuildInterviewQuestionView(textListQuestionTemplate, 
            textListQuestionData, null, false, new decimal[0], InterviewStatus.Completed);

        It should_answer_be_null = () =>
            textListQuestionView.Answer.ShouldBeNull();

        It should_have_options_not_null = () =>
            textListQuestionView.Options.Count.ShouldEqual(2);
        
        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView textListQuestionView;
        private static SingleQuestion textListQuestionTemplate;
        private static InterviewQuestion textListQuestionData;
        private static Guid textListQuestionId = Guid.Parse("44444444444444444444444444444444");
    }
}
