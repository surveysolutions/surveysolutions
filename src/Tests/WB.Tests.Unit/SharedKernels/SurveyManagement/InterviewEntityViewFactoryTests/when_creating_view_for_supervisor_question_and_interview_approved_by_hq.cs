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
    internal class when_creating_view_for_supervisor_question_and_interview_approved_by_hq : InterviewEntityViewFactoryTestsContext
    {
        Establish context = () =>
        {
            question = Create.Entity.Question(); 
            question.QuestionScope = QuestionScope.Supervisor;
            interviewEntityViewFactory = CreateInterviewEntityViewFactory();
        };

        Because of = () =>
            result =
                interviewEntityViewFactory.BuildInterviewQuestionView(question: question, answeredQuestion: null,
                    answersForTitleSubstitution: new Dictionary<string, string>(), 
                    isParentGroupDisabled: false,
                    rosterVector: new decimal[0],
                    interviewStatus: InterviewStatus.ApprovedByHeadquarters);
        
        It should_set_readonly_flag_to_true = () =>
            result.IsReadOnly.ShouldBeTrue();

        private static IInterviewEntityViewFactory interviewEntityViewFactory;
        private static InterviewQuestionView result;
        private static IQuestion question;
    }
}