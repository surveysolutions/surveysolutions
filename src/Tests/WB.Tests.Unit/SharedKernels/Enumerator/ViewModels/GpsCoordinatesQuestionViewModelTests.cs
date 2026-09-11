using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NSubstitute;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(GpsCoordinatesQuestionViewModel))]
    public class GpsCoordinatesQuestionViewModelTests : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public void should_raise_can_execute_changed_when_answering_in_progress_changes()
        {
            var answering = new AnsweringViewModel(
                Mock.Of<ICommandService>(),
                Mock.Of<IUserInterfaceStateService>(),
                Mock.Of<ILogger>());

            var viewModel = new GpsCoordinatesQuestionViewModel(
                Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Guid.NewGuid())),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IEnumeratorSettings>(),
                Mock.Of<IGpsLocationService>(),
                Create.ViewModel.QuestionState<GeoLocationQuestionAnswered>(),
                Mock.Of<IUserInterfaceStateService>(),
                answering,
                Create.ViewModel.QuestionInstructionViewModel(),
                Create.Service.LiteEventRegistry(),
                Mock.Of<ILogger>(),
                Mock.Of<IGoogleApiService>(),
                Mock.Of<IExternalAppLauncher>());

            var canExecuteChangedRaised = 0;
            viewModel.SaveAnswerCommand.CanExecuteChanged += (_, _) => canExecuteChangedRaised++;

            answering.StartInProgressIndicator();
            answering.FinishInProgressIndicator();

            Assert.That(canExecuteChangedRaised, Is.EqualTo(2));
            Assert.That(viewModel.SaveAnswerCommand.CanExecute(), Is.True);
        }

        [Test]
        public async Task should_not_answer_question_when_disposed_while_waiting_for_location()
        {
            var commandService = new Mock<ICommandService>();
            var answering = new AnsweringViewModel(
                commandService.Object,
                Mock.Of<IUserInterfaceStateService>(),
                Mock.Of<ILogger>());

            var locationRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var location = new TaskCompletionSource<GpsLocation>(TaskCreationOptions.RunContinuationsAsynchronously);
            var locationService = new Mock<IGpsLocationService>();
            locationService
                .Setup(x => x.GetLocation(It.IsAny<double>(), It.IsAny<AcceptableGpsLocationSource>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    locationRequested.TrySetResult(true);
                    return location.Task;
                });

            var viewModel = new GpsCoordinatesQuestionViewModel(
                Mock.Of<IPrincipal>(x => x.CurrentUserIdentity == Mock.Of<IUserIdentity>(u => u.UserId == Guid.NewGuid())),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IEnumeratorSettings>(x => x.GpsReceiveTimeoutSec == 60),
                locationService.Object,
                Substitute.For<QuestionStateViewModel<GeoLocationQuestionAnswered>>(),
                Mock.Of<IUserInterfaceStateService>(),
                answering,
                Create.ViewModel.QuestionInstructionViewModel(),
                Create.Service.LiteEventRegistry(),
                Mock.Of<ILogger>(),
                Mock.Of<IGoogleApiService>(),
                Mock.Of<IExternalAppLauncher>());
            viewModel.IsEditMode = true;

            var saveTask = viewModel.SaveAnswerCommand.ExecuteAsync();
            await locationRequested.Task;

            viewModel.Dispose();

            // a fallback location may still be reported after the wait was canceled by disposal
            location.SetResult(new GpsLocation(10, 100, 1, 2, DateTimeOffset.UtcNow, "network"));
            await saveTask;

            commandService.Verify(
                x => x.ExecuteAsync(It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
