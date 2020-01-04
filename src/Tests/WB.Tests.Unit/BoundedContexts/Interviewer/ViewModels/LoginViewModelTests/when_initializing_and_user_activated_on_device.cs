using System.Threading.Tasks;
using FluentAssertions;

using Moq;

using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_and_user_activated_on_device : LoginViewModelTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public async Task context () {
            var interview = Substitute.For<IStatefulInterview>();

            var interviewer = CreateInterviewerIdentity(userName);

            InterviewerPrincipal
               .Setup(x => x.DoesIdentityExist())
               .Returns(true);

            InterviewerPrincipal
                .Setup(x => x.GetExistingIdentityNameOrNull())
                .Returns(interviewer.Name);

            viewModel = CreateLoginViewModel(
                viewModelNavigationService: ViewModelNavigationServiceMock.Object,
                principal: InterviewerPrincipal.Object);
            await BecauseOf();
        }

        public async Task BecauseOf() => await viewModel.Initialize();

        [NUnit.Framework.Test] public void should_fill_user_name () =>
            viewModel.UserName.Should().Be(userName);

        [NUnit.Framework.Test] public void should_set_IsUserValid_in_true () =>
            viewModel.IsUserValid.Should().BeTrue();

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
        static Mock<IViewModelNavigationService> ViewModelNavigationServiceMock = new Mock<IViewModelNavigationService>();
        static Mock<IInterviewerPrincipal> InterviewerPrincipal = new Mock<IInterviewerPrincipal>();
    }
}
