using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewEntityViewFactoryTests
{
    internal class when_creating_InterviewQuestionView_for_not_answered_TextList_question : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            textListQuestionTemplate = new TextListQuestion("text list");
            textListQuestionData = new InterviewQuestion(textListQuestionId)
            {
                Answer = new InterviewTextListAnswers(new List<Tuple<decimal, string>>())
            };
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () => textListQuestionView = interviewEntityViewFactory.BuildInterviewQuestionView(textListQuestionTemplate, 
            textListQuestionData, null, false, new decimal[0], InterviewStatus.Completed);

        It should_answer_be_null = () =>
            textListQuestionView.Answer.ShouldBeNull();

        It should_have_options_not_null = () =>
            textListQuestionView.Options.ShouldNotBeNull();
        
        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView textListQuestionView;
        private static TextListQuestion textListQuestionTemplate;
        private static InterviewQuestion textListQuestionData;
        private static Guid textListQuestionId = Guid.Parse("44444444444444444444444444444444");
    }
}
