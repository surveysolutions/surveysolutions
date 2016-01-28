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
    internal class when_creating_view_for_supervisor_question_and_interview_approved_by_hq
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
                    interviewStatus: InterviewStatus.ApprovedByHeadquarters);
        
        It should_set_readonly_flag_to_true = () =>
            result.IsReadOnly.ShouldBeTrue();

        private static InterviewQuestionView result;
        private static IQuestion question;
    }
}