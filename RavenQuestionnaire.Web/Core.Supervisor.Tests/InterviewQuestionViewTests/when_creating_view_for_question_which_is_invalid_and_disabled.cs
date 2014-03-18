using System;
using System.Collections.Generic;
using Core.Supervisor.Views.Interview;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using It = Machine.Specifications.It;

namespace Core.Supervisor.Tests.InterviewQuestionViewTests
{
    internal class when_creating_view_for_question_which_is_invalid_and_disabled
    {
        Establish context = () =>
        {
            question = Mock.Of<IQuestion>();

            answeredQuestion = new InterviewQuestion { Valid = false, Enabled = false };
        };

        Because of = () =>
            result = new InterviewQuestionView(question, answeredQuestion, new Dictionary<Guid, string>(), new Dictionary<string, string>(), isParentGroupDisabled: false);

        It should_set_validity_flag_to_valid = () =>
            result.IsValid.ShouldBeTrue();

        private static InterviewQuestionView result;
        private static IQuestion question;
        private static InterviewQuestion answeredQuestion;
    }
}