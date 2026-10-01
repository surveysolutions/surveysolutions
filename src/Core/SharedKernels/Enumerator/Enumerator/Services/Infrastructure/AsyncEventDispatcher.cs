using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class AsyncEventDispatcher : IAsyncEventDispatcher
    {
        private readonly IViewModelEventRegistry viewModelEventRegistry;
        private readonly ILogger logger;
        private readonly ICurrentViewModelPresenter currentViewModelPresenter;
        private readonly IUserInterfaceStateService userInterfaceStateService;

        public AsyncEventDispatcher(IViewModelEventRegistry viewModelEventRegistry,
            ILogger logger,
            ICurrentViewModelPresenter currentViewModelPresenter,
            IUserInterfaceStateService userInterfaceStateService)
        {
            this.viewModelEventRegistry = viewModelEventRegistry;
            this.logger = logger;
            this.currentViewModelPresenter = currentViewModelPresenter;
            this.userInterfaceStateService = userInterfaceStateService;
        }

        public async Task ExecuteAsync(IReadOnlyCollection<CommittedEvent> events)
        {
            this.userInterfaceStateService.NotifyRefreshStarted();
            try
            {
                foreach (var @event in events)
                foreach (var viewModel in this.viewModelEventRegistry.GetViewModelsByEvent(@event))
                {
                    var eventType = @event.Payload.GetType();
                    var viewModelType = viewModel.GetType();

                    var handler = this.viewModelEventRegistry.GetViewModelHandleMethod(viewModelType, eventType);

                    try
                    {
                        var taskOrVoid = (Task) handler?.Invoke(viewModel, new object[] {@event.Payload});
                        if (taskOrVoid != null) await taskOrVoid;
                    }
                    catch (Exception e)
                    {
                        this.logger.Error($"Unhandled exception in {viewModelType.Name}.{handler?.Name}<{eventType.Name}>", e);

                        ((BaseInterviewViewModel) this.currentViewModelPresenter.CurrentViewModel)?.ReloadCommand
                            ?.Execute();

                        return;
                    }
                }
            }
            finally
            {
                this.userInterfaceStateService.NotifyRefreshFinished();
            }
        }
    }
}
