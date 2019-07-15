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
            try
            {
                foreach (var @event in events)
                foreach (var viewModel in this.viewModelEventRegistry.GetViewModelsByEvent(@event))
                {
                    var isAsyncHandler = viewModel
                        .GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Any(type =>
                            type.IsGenericType && type.GetGenericTypeDefinition() ==
                            typeof(IAsyncViewModelEventHandler<>));

                    var methodName = $"Handle{(isAsyncHandler ? "Async" : "")}";

                    var handler = viewModel.GetType()
                        .GetRuntimeMethod(methodName, new[] {@event.Payload.GetType()});

                    var taskOrVoid = (Task) handler?.Invoke(viewModel, new object[] {@event.Payload});
                    if (taskOrVoid != null) await taskOrVoid;
                }
            }
            catch (Exception e)
            {
                ((BaseInterviewViewModel) this.currentViewModelPresenter.CurrentViewModel)?.ReloadCommand?.Execute();

                this.logger.Error("Exception during update view models", e);
            }
        }
    }
}
