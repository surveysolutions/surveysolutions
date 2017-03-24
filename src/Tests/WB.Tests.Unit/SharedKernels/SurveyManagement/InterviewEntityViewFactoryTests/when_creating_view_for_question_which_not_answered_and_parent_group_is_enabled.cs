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
    internal class when_creating_view_for_question_which_not_answered_and_parent_group_is_enabled : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            question = Create.Entity.Question();
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () =>
            result =
                interviewEntityViewFactory.BuildInterviewQuestionView(question: question, answeredQuestion: null,
                    answersForTitleSubstitution: new Dictionary<string, string>(), isParentGroupDisabled: false,
                    rosterVector:new decimal[0],
                    interviewStatus: InterviewStatus.Completed);

        It should_set_enabled_flag_to_true = () =>
            result.IsEnabled.ShouldBeTrue();

        It should_set_readonly_flag_to_true = () =>
            result.IsReadOnly.ShouldBeTrue();

        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView result;
        private static IQuestion question;
    }
}