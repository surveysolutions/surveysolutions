using System;
using System.Threading.Tasks;
using Moq;
using MvvmCross.Commands;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services
{
    [TestOf(typeof(AsyncEventDispatcher))]
    public class InterviewViewModelEventsPublisherTests
    {
        public class FakeViewModelWithException : 
            IAsyncViewModelEventHandler<TextQuestionAnswered>,
            IViewModelEventHandler<NumericIntegerQuestionAnswered>
        {
            public virtual Task HandleAsync(TextQuestionAnswered @event)
            {
                throw new NotImplementedException();
            }

            public void Handle(NumericIntegerQuestionAnswered @event)
            {
                throw new ArgumentNullException();
            }
        }

        public class FakeViewModel : 
            IAsyncViewModelEventHandler<TextQuestionAnswered>,
            IViewModelEventHandler<NumericIntegerQuestionAnswered>
        {
            public bool IsHandleExecuted { get; private set; } = false;
            public bool IsHandleAsyncExecuted { get; private set; } = false;

            public virtual Task HandleAsync(TextQuestionAnswered @event)
            {
                IsHandleAsyncExecuted = true;
                return Task.CompletedTask;
            }

            public void Handle(NumericIntegerQuestionAnswered @event) => IsHandleExecuted = true;
        }

        public class FakeInterviewViewModel : BaseInterviewViewModel
        {
            public FakeInterviewViewModel() : base(null, null, null, 
                null, Create.Other.NavigationState(), null, null,
                null, null, Create.Service.InterviewerPrincipal(Guid.NewGuid()), 
                Mock.Of<IViewModelNavigationService>(), null, null, null, null)
            {
            }

            public override IMvxCommand ReloadCommand { get; }
            public override Task NavigateBack()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public async Task when_ExecuteAsync_and_view_model_throw_unhandled_exception_then_exception_should_be_logged_and_interview_should_be_reloaded()
        {
            // arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var eventRegistry = Create.Service.LiteEventRegistry();
            var interviewViewModel = new Mock<FakeInterviewViewModel>();
            var mockOfReloadCommand = new Mock<IMvxCommand>();
            interviewViewModel.SetupGet(x => x.ReloadCommand).Returns(mockOfReloadCommand.Object);

            var mockOfCurrentViewModelPresenter = new Mock<ICurrentViewModelPresenter>();
            mockOfCurrentViewModelPresenter.Setup(x => x.CurrentViewModel).Returns(interviewViewModel.Object);
            var mockOfLogger = new Mock<ILogger>();

            var viewModel = new FakeViewModelWithException();
            eventRegistry.Subscribe(viewModel, interviewId.ToString("N"));

            var publisher = Create.Service.InterviewViewModelEventsPublisher(eventRegistry,
                mockOfLogger.Object, mockOfCurrentViewModelPresenter.Object);

            var events = new[]
            {
                Create.Other.CommittedEvent(payload:Create.Event.TextQuestionAnswered(), eventSourceId: interviewId),
                Create.Other.CommittedEvent(payload:Create.Event.NumericIntegerQuestionAnswered(), eventSourceId: interviewId)
            };
            
            // act
            await publisher.ExecuteAsync(events);
            // assert
            mockOfReloadCommand.Verify(x => x.Execute(), Times.Once);
            mockOfLogger.Verify(x =>
                x.Error($"Unhandled exception in {nameof(FakeViewModelWithException)}.HandleAsync<{nameof(TextQuestionAnswered)}>",
                    Moq.It.Is<Exception>(y => y.InnerException is NotImplementedException)), Times.Once);
        }

        [Test]
        public async Task when_ExecuteAsync_and_view_model_has_async_and_sync_handle_methods_then_sync_and_async_methods_should_be_invoked()
        {
            // arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var eventRegistry = Create.Service.LiteEventRegistry();

            var events = new[]
            {
                Create.Other.CommittedEvent(payload:Create.Event.TextQuestionAnswered(), eventSourceId: interviewId),
                Create.Other.CommittedEvent(payload:Create.Event.NumericIntegerQuestionAnswered(), eventSourceId: interviewId)
            };

            var viewModel = new FakeViewModel();
            eventRegistry.Subscribe(viewModel, interviewId.ToString("N"));

            var publisher = Create.Service.InterviewViewModelEventsPublisher(eventRegistry);

            // act
            await publisher.ExecuteAsync(events);
            // assert
            Assert.That(viewModel.IsHandleAsyncExecuted, Is.True);
            Assert.That(viewModel.IsHandleExecuted, Is.True);
        }
    }
}
