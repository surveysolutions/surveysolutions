using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    internal class when_creating_InterviewQuestionView_for_TextList_question
    {
        Establish context = () =>
        {
            textListQuestionTemplate = new TextListQuestion("text list");
            textListQuestionData = new InterviewQuestion(textListQuestionId);
            textListQuestionData.Answer =
                new InterviewTextListAnswers(new[] { new Tuple<decimal, string>(1, "q1"), new Tuple<decimal, string>(2, "q2") });
        };

        Because of = () => textListQuestionView = new InterviewQuestionView(textListQuestionTemplate, textListQuestionData,
               null, null, false, new decimal[0], InterviewStatus.Completed);

        It should_answer_be_null = () =>
            textListQuestionView.Answer.ShouldBeNull();

        It should_have_2_options = () =>
            textListQuestionView.Options.Count.ShouldEqual(2);

        It should_first_option_value_equal_1 = () =>
            ((decimal)textListQuestionView.Options[0].Value).ShouldEqual(1);

        It should_first_option_answer_equal_1 = () =>
            textListQuestionView.Options[0].Label.ShouldEqual("q1");

        private static InterviewQuestionView textListQuestionView;
        private static TextListQuestion textListQuestionTemplate;
        private static InterviewQuestion textListQuestionData;
        private static Guid textListQuestionId = Guid.Parse("44444444444444444444444444444444");
    }
}
