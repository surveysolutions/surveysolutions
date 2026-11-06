using System;
using System.Threading.Tasks;
using MvvmCross.Base;
using FluentAssertions;
using Moq;
using MvvmCross.Commands;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;
using InterviewerInterviewViewModel = WB.UI.Interviewer.ViewModel.InterviewViewModel;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.ViewModels
{
    [TestFixture]
    [TestOf(typeof(BaseInterviewViewModel))]
    public class InterviewViewModelTests : MvxIoCSupportingTest
    {
        public InterviewViewModelTests()
        {
            base.Setup();
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }

        [Test]
        public void when_complete_vm_finishes_loading_status_should_be_updated_from_complete_vm()
        {
            // arrange
            var completeVm = new FakeCompleteViewModel { CompleteStatus = GroupStatus.CompletedInvalid };
            var factory = CreateFactoryReturning(completeVm);
            var navState = CreateNavigationStateOnCompleteScreen();
            var sut = new TestableInterviewViewModel(factory, navState);

            sut.SimulateNavigateToComplete();

            // act — simulate background criticality check finishing
            completeVm.IsLoading = false;

            // assert
            sut.Status.Should().Be(GroupStatus.CompletedInvalid);
        }

        [Test]
        public void when_stale_complete_vm_finishes_loading_after_navigation_away_status_should_not_change()
        {
            // arrange
            var staleCompleteVm = new FakeCompleteViewModel { CompleteStatus = GroupStatus.CompletedInvalid };
            var factory = CreateFactoryReturning(staleCompleteVm);
            var navState = CreateNavigationStateOnCompleteScreen();
            var sut = new TestableInterviewViewModel(factory, navState);

            sut.SimulateNavigateToComplete();

            // simulate navigating away — the next UpdateCurrentScreenViewModel call for a non-Complete
            // screen unsubscribes from the old VM
            navState.CurrentScreenType.Returns(ScreenType.Identifying);
            sut.SimulateNavigateToCurrentScreen();

            // act — stale notification arrives from the old complete VM
            var statusBeforeStaleUpdate = sut.Status;
            staleCompleteVm.IsLoading = false;

            // assert — Status must not have changed because the subscription was cleared
            sut.Status.Should().Be(statusBeforeStaleUpdate);
        }

        [Test]
        public void when_navigating_away_from_complete_screen_subscription_should_be_removed_from_old_vm()
        {
            // arrange
            var firstCompleteVm = new FakeCompleteViewModel();
            var factory = CreateFactoryReturning(firstCompleteVm);
            var navState = CreateNavigationStateOnCompleteScreen();
            var sut = new TestableInterviewViewModel(factory, navState);

            sut.SimulateNavigateToComplete();
            int handlerCount = firstCompleteVm.PropertyChangedHandlerCount;

            // act — navigate away from the Complete screen
            navState.CurrentScreenType.Returns(ScreenType.Identifying);
            sut.SimulateNavigateToCurrentScreen();

            // assert — PropertyChanged handler was removed
            firstCompleteVm.PropertyChangedHandlerCount.Should().BeLessThan(handlerCount);
        }

        [Test]
        public void when_navigating_back_to_complete_screen_only_new_vm_handler_should_fire()
        {
            // arrange
            var firstCompleteVm = new FakeCompleteViewModel { CompleteStatus = GroupStatus.CompletedInvalid };
            var secondCompleteVm = new FakeCompleteViewModel { CompleteStatus = GroupStatus.Completed };

            var factory = Substitute.For<IInterviewViewModelFactory>();
            factory.GetNew<CompleteInterviewViewModel>().Returns(firstCompleteVm, secondCompleteVm);

            var navState = CreateNavigationStateOnCompleteScreen();
            var sut = new TestableInterviewViewModel(factory, navState);

            sut.SimulateNavigateToComplete();        // subscribes to firstCompleteVm

            navState.CurrentScreenType.Returns(ScreenType.Identifying);
            sut.SimulateNavigateToCurrentScreen();  // unsubscribes from firstCompleteVm

            navState.CurrentScreenType.Returns(ScreenType.Complete);
            sut.SimulateNavigateToComplete();       // subscribes to secondCompleteVm

            // act — second VM finishes loading; first also fires (stale)
            secondCompleteVm.IsLoading = false;
            firstCompleteVm.IsLoading = false;

            // assert — Status reflects the second VM, not the first (stale) one
            sut.Status.Should().Be(GroupStatus.Completed);
        }

        // ── helpers ─────────────────────────────────────────────────────────────────────────────

        private static IInterviewViewModelFactory CreateFactoryReturning(CompleteInterviewViewModel vm)
        {
            var factory = Substitute.For<IInterviewViewModelFactory>();
            factory.GetNew<CompleteInterviewViewModel>().Returns(vm);
            return factory;
        }

        private static NavigationState CreateNavigationStateOnCompleteScreen()
        {
            var navState = Substitute.For<NavigationState>();
            navState.CurrentScreenType.Returns(ScreenType.Complete);
            return navState;
        }

        // ── test doubles ─────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Minimal <see cref="CompleteInterviewViewModel"/> that can be constructed without any real
        /// dependencies. <see cref="Configure"/> is a no-op so tests can control <see cref="IsLoading"/>
        /// and <see cref="CompleteStatus"/> directly.
        /// </summary>
        private class FakeCompleteViewModel : CompleteInterviewViewModel
        {
            public FakeCompleteViewModel() : base(null, null, null, null, null, null, null, null, null, null) { }

            public override void Configure(string interviewId, NavigationState navigationState) { /* no-op for testing */ }

            public int PropertyChangedHandlerCount
            {
                get
                {
                    // MvvmCross stores PropertyChanged as a regular C# event field on MvxNotifyPropertyChanged.
                    var propertyChangedField = typeof(MvvmCross.ViewModels.MvxNotifyPropertyChanged)
                        .GetField("PropertyChanged",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    var del = propertyChangedField?.GetValue(this) as System.Delegate;
                    return del?.GetInvocationList().Length ?? 0;
                }
            }

        }

        /// <summary>
        /// Concrete subclass of <see cref="BaseInterviewViewModel"/> used in tests to exercise the
        /// stale-subscription guard implemented in the base class.
        /// </summary>
        private class TestableInterviewViewModel : BaseInterviewViewModel
        {
            public TestableInterviewViewModel(IInterviewViewModelFactory factory, NavigationState navState)
                : base(null, null, null, null, navState, null, null, null, null,
                    Mock.Of<IPrincipal>(), Mock.Of<IViewModelNavigationService>(),
                    factory, null, null, null)
            {
            }

            public override IMvxCommand ReloadCommand { get; }
            public override Task NavigateBack() => Task.CompletedTask;

            public new GroupStatus Status => base.Status;

            /// <summary>Directly invokes <see cref="UpdateCurrentScreenViewModel"/> for the current screen type.</summary>
            public void SimulateNavigateToCurrentScreen()
            {
                // Tests configure a non-Complete screen before calling this helper. Identifying hits the
                // default branch in the base switch and avoids extra factory mocks.
                var eventArgs = new ScreenChangedEventArgs(NavigationState.CurrentScreenType, null, null, ScreenType.Complete, null);
                UpdateCurrentScreenViewModel(eventArgs);
            }

            /// <summary>Navigates to Complete screen and invokes <see cref="UpdateCurrentScreenViewModel"/>.</summary>
            public void SimulateNavigateToComplete()
            {
                NavigationState.CurrentScreenType.Returns(ScreenType.Complete);
                var eventArgs = new ScreenChangedEventArgs(ScreenType.Complete, null, null, ScreenType.Group, null);
                UpdateCurrentScreenViewModel(eventArgs);
            }
        }

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

                Action act = () => sut.InvokeApplyCompleteViewModelStatus(Create.ViewModel.CompleteInterviewViewModel());

                act.Should().Throw<InvalidOperationException>();
            }

            [Test]
            public void when_already_on_main_thread_apply_complete_status_should_not_dispatch()
            {
                var sut = new TestableInterviewerInterviewViewModel(new MainThreadDispatcherThatMustNotBeCalled());

                Action act = () => sut.InvokeApplyCompleteViewModelStatus(Create.ViewModel.CompleteInterviewViewModel());

                act.Should().NotThrow();
            }

            private class TestableInterviewerInterviewViewModel : InterviewerInterviewViewModel
            {
                public TestableInterviewerInterviewViewModel(IMvxMainThreadAsyncDispatcher dispatcher)
                    : base(null, null, null, null, Substitute.For<NavigationState>(), null,
                        Mock.Of<IViewModelNavigationService>(), Mock.Of<IPrincipal>(),
                        null, null, null, Substitute.For<IInterviewViewModelFactory>(), null, null, null,
                        null, null, Mock.Of<ILogger>(), null, null, dispatcher,
                        Mock.Of<IAudioAuditRecordingExecutor>())
                {
                }

                public void InvokeApplyCompleteViewModelStatus(CompleteInterviewViewModel vm)
                {
                    this.ApplyCompleteViewModelStatus(vm);
                }

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
}
