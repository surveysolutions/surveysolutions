using Machine.Specifications;
using NSubstitute;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Aggregates;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels.LoginViewModelTests
{
    public class when_initializing_login_view_model : LoginViewModelTestContext
    {
        Establish context = () =>
        {
            var interview = Substitute.For<IStatefulInterview>();
            interview.CreatedOnClient.Returns(true);

            var interviewer = CreateInterviewerIdentity(userName);

            viewModel = CreateLoginViewModel(interviewer: interviewer);
        };

        Because of = () => viewModel.Init();

        It should_fill_user_name = () =>
            viewModel.UserName.ShouldEqual(userName);

        It should_set_IsUserValid_in_true = () =>
            viewModel.IsUserValid.ShouldBeTrue();

        static LoginViewModel viewModel;
        private static readonly string userName = "Vasya";
    }
}