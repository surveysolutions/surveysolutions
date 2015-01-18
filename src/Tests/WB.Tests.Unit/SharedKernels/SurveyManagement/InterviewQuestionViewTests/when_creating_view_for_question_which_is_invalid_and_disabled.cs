using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewQuestionViewTests
{
    using System.Linq.Expressions;

    internal class when_creating_view_for_question_which_is_invalid_and_disabled
    {
        Establish context = () =>
        {
            question = Mock.Of<IQuestion>();

            answeredQuestion = new InterviewQuestion {QuestionState = QuestionState.Valid | QuestionState.Valid};
        };

        Because of = () =>
            result = new InterviewQuestionView(question, answeredQuestion, new Dictionary<Guid, string>(), new Dictionary<string, string>(), isParentGroupDisabled: false, rosterVector: new decimal[0]);

        It should_set_validity_flag_to_valid = () =>
            result.IsValid.ShouldBeTrue();

        private static InterviewQuestionView result;
        private static IQuestion question;
        private static InterviewQuestion answeredQuestion;
    }
}