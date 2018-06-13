using FluentAssertions;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.NavigationStateTests
{
    internal class when_initializing_navigation_state
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            navigationState = Create.Other.NavigationState();
            BecauseOf();
        }

        public void BecauseOf() =>
            navigationState.Init(interviewId, questionnaireId);

        [NUnit.Framework.Test] public void should_set_InterviewId_as_specified () =>
            navigationState.InterviewId.Should().Be(interviewId);

        [NUnit.Framework.Test] public void should_set_QuestionnaireId_as_specified () =>
            navigationState.QuestionnaireId.Should().Be(questionnaireId);

        private static NavigationState navigationState;
        private static readonly string interviewId = "Some interviewId";
        private static readonly string questionnaireId = "Questionnaire Id";
    }
}
