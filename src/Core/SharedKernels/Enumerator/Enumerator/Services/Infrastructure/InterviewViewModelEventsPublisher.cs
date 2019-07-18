using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class InterviewViewModelEventsPublisher : IBackgroundJob<IEnumerable<CommittedEvent>>
    {
        private readonly IViewModelEventRegistry viewModelEventRegistry;
        private readonly ILogger logger;
        private readonly ICurrentViewModelPresenter currentViewModelPresenter;

        public InterviewViewModelEventsPublisher(IViewModelEventRegistry viewModelEventRegistry,
            ILogger logger,
            ICurrentViewModelPresenter currentViewModelPresenter)
        {
            this.viewModelEventRegistry = viewModelEventRegistry;
            this.logger = logger;
            this.currentViewModelPresenter = currentViewModelPresenter;
        }

        public async Task ExecuteAsync(IEnumerable<CommittedEvent> events)
        {
            foreach (var @event in events)
            foreach (var viewModel in this.viewModelEventRegistry.GetViewModelsByEvent(@event))
            {
                var eventType = @event.Payload.GetType();
                var viewModelType = viewModel.GetType();

                var methodName = $"Handle{(this.viewModelEventRegistry.IsAsyncViewModelHandleMethod(viewModelType, eventType) ? "Async" : "")}";

                var handler = viewModelType
                    .GetRuntimeMethod(methodName, new[] { eventType });

                try
                {
                    var taskOrVoid = (Task) handler?.Invoke(viewModel, new object[] {@event.Payload});
                    if (taskOrVoid != null) await taskOrVoid;
                }
                catch (Exception e)
                {
                    this.logger.Error($"Unhandled exception in {viewModelType.Name}.{methodName}<{eventType.Name}>", e);

                    ((BaseInterviewViewModel) this.currentViewModelPresenter.CurrentViewModel)?.ReloadCommand?.Execute();

                    return;
                }
            }

        }
    }
}
