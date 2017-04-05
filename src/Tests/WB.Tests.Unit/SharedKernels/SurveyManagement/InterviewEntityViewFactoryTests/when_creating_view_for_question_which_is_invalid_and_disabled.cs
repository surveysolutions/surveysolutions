using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    internal class when_creating_view_for_question_which_is_invalid_and_disabled : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            question = Create.Entity.Question();

            answeredQuestion = new InterviewQuestion {QuestionState = QuestionState.Valid | QuestionState.Valid};
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () =>
            result = interviewEntityViewFactory.BuildInterviewQuestionView(question, answeredQuestion, new Dictionary<string, string>(), 
                isParentGroupDisabled: false, 
                rosterVector: new decimal[0], 
                interviewStatus: InterviewStatus.Completed);

        It should_set_validity_flag_to_valid = () =>
            result.IsValid.ShouldBeTrue();

        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView result;
        private static IQuestion question;
        private static InterviewQuestion answeredQuestion;
    }
}