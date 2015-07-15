using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.ViewModels;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.NavigationStateTests
{
    public class when_initializing_navigation_state
    {
        Establish context = () =>
        {
            navigationState = Create.NavigationState();
        };

        Because of = () =>
            navigationState.Init(interviewId, questionnaireId);

        It should_set_InterviewId_as_specified = () =>
            navigationState.InterviewId.ShouldEqual(interviewId);

        It should_set_QuestionnaireId_as_specified = () =>
            navigationState.QuestionnaireId.ShouldEqual(questionnaireId);

        private static NavigationState navigationState;
        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";
    }
}