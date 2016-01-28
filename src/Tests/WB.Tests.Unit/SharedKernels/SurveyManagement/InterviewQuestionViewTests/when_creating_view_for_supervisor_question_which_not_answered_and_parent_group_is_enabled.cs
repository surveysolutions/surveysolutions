using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    internal class when_creating_view_for_supervisor_question_which_not_answered_and_parent_group_is_enabled
    {
        Establish context = () =>
        {
            question = Mock.Of<IQuestion>();
            question.QuestionScope = QuestionScope.Supervisor;
        };

        Because of = () =>
            result =
                new InterviewQuestionView(question: question, answeredQuestion: null,
                    variablesMap: new Dictionary<Guid, string>(),
                    answersForTitleSubstitution: new Dictionary<string, string>(), 
                    isParentGroupDisabled: false,
                    rosterVector: new decimal[0],
                    interviewStatus: InterviewStatus.Completed);

        It should_set_enabled_flag_to_true = () =>
            result.IsEnabled.ShouldBeTrue();

        It should_set_readonly_flag_to_false = () =>
            result.IsReadOnly.ShouldBeFalse();

        private static InterviewQuestionView result;
        private static IQuestion question;
    }
}