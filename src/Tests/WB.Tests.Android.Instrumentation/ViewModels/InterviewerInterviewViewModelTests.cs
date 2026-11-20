using System;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Base;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using InterviewerInterviewViewModel = WB.UI.Interviewer.ViewModel.InterviewViewModel;

namespace WB.Tests.Android.Instrumentation.ViewModels
{
    [TestFixture]
    [TestOf(typeof(InterviewerInterviewViewModel))]
    public class InterviewerInterviewViewModelTests : MvxIoCSupportingTest
    {
        public InterviewerInterviewViewModelTests()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public void when_dispatcher_fails_while_applying_complete_status_exception_should_be_observed()
        {
            var sut = new TestableInterviewerInterviewViewModel(new BackgroundFaultingDispatcher());

            Assert.Throws<InvalidOperationException>(() =>
                sut.InvokeApplyCompleteViewModelStatus(CreateCompleteViewModel()));
        }

        [Test]
        public void when_already_on_main_thread_apply_complete_status_should_not_dispatch()
        {
            var sut = new TestableInterviewerInterviewViewModel(new MainThreadDispatcherThatMustNotBeCalled());

            Assert.DoesNotThrow(() =>
                sut.InvokeApplyCompleteViewModelStatus(CreateCompleteViewModel()));
        }

        private static CompleteInterviewViewModel CreateCompleteViewModel()
            => new(null, null, null, null, null, null, null, null, null, null);

        private class TestableInterviewerInterviewViewModel : InterviewerInterviewViewModel
        {
            public TestableInterviewerInterviewViewModel(IMvxMainThreadAsyncDispatcher dispatcher)
                : base(null, null, null, null, new NavigationState(null, null, null), null,
                    null, null, null, null, null, null, null, null, null,
                    null, null, null, null, null, dispatcher, null)
            {
            }

            public void InvokeApplyCompleteViewModelStatus(CompleteInterviewViewModel vm)
                => this.ApplyCompleteViewModelStatus(vm);

            public override IMvxCommand ReloadCommand { get; }
            public override Task NavigateBack() => Task.CompletedTask;
        }

        private class BackgroundFaultingDispatcher : IMvxMainThreadAsyncDispatcher
        {
            public Task ExecuteOnMainThreadAsync(Action action, bool maskExceptions = true)
            {
                action();
                return Task.FromException(new InvalidOperationException("dispatcher failure"));
            }

            public Task ExecuteOnMainThreadAsync(Func<Task> action, bool maskExceptions = true)
                => Task.FromException(new InvalidOperationException("dispatcher failure"));

            public bool IsOnMainThread => false;
        }

        private class MainThreadDispatcherThatMustNotBeCalled : IMvxMainThreadAsyncDispatcher
        {
            public Task ExecuteOnMainThreadAsync(Action action, bool maskExceptions = true)
                => throw new InvalidOperationException("dispatcher should not be used on the main thread");

            public Task ExecuteOnMainThreadAsync(Func<Task> action, bool maskExceptions = true)
                => throw new InvalidOperationException("dispatcher should not be used on the main thread");

            public bool IsOnMainThread => true;
        }
    }
}
