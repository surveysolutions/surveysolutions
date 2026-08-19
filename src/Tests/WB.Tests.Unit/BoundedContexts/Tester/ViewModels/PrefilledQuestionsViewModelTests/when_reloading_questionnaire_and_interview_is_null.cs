using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MvvmCross.Commands;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.PrefilledQuestionsViewModelTests
{
    internal class when_reloading_questionnaire_and_interview_is_null : PrefilledQuestionsViewModelTestContext
    {
        [Test]
        public async Task should_navigate_to_dashboard_and_not_call_reload()
        {
            string interviewId = "11111111111111111111111111111111";

            var interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();
            interviewRepositoryMock.Setup(r => r.Get(interviewId)).Returns(default(IStatefulInterview));

            var navigationServiceMock = new Mock<IViewModelNavigationService>();
            navigationServiceMock
                .Setup(ns => ns.NavigateToDashboardAsync(null))
                .Returns(Task.FromResult(true));

            // questionnaireDownloader is null — any call to it would throw NullReferenceException,
            // confirming the early-exit path is taken correctly when it does not throw
            var viewModel = CreatePrefilledQuestionsViewModel(
                interviewRepository: interviewRepositoryMock.Object,
                viewModelNavigationService: navigationServiceMock.Object);

            viewModel.Prepare(new InterviewViewModelArgs { InterviewId = interviewId });

            // Simulate viewmodel having been previously initialized (PrefilledQuestions is set)
            viewModel.PrefilledQuestions = new CompositeCollection<ICompositeEntity>();

            // Act
            await viewModel.ReloadQuestionnaireCommand.ExecuteAsync();

            // Assert
            navigationServiceMock.Verify(ns => ns.NavigateToDashboardAsync(null), Times.Once,
                "should navigate to dashboard when interview is null at the start of reload");
        }
    }
}
