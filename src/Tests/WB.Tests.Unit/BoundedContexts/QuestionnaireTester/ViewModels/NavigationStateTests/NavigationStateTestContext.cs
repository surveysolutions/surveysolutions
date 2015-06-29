using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.CommandBus;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.NavigationStateTests
{
    public class NavigationStateTestContext
    {
        public static NavigationState CreateNavigationState(ICommandService commandService = null)
        {
            return new NavigationState(commandService);
        }

        protected static readonly string interviewId = "Some interviewId";
        protected static readonly string questionnaireId = "Questionnaire Id";
    }

    public class when_initializing_navigation_state : NavigationStateTestContext
    {
        Establish context = () =>
        {
            navigationState = CreateNavigationState();
        };

        Because of = () =>
            navigationState.Init(interviewId, questionnaireId);

        It should_set_InterviewId_as_specified = () =>
            navigationState.InterviewId.ShouldEqual(interviewId);

        It should_set_QuestionnaireId_as_specified = () =>
            navigationState.QuestionnaireId.ShouldEqual(questionnaireId);

        static NavigationState navigationState;
    }
}
