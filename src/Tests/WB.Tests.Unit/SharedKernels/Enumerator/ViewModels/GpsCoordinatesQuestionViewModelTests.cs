using System;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
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
    }
}
