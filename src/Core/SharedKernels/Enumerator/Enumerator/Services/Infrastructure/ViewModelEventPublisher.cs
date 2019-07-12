using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Services;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    internal class ViewModelEventPublisher : IBackgroundJob<CommittedEvent>
    {
        private readonly IViewModelEventRegistry viewModelEventRegistry;
        private readonly ILogger logger;

        public ViewModelEventPublisher(IViewModelEventRegistry viewModelEventRegistry,
            ILogger logger)
        {
            this.viewModelEventRegistry = viewModelEventRegistry;
            this.logger = logger;
        }

        public async Task ExecuteAsync(CommittedEvent item)
        {
            foreach (var viewModel in this.viewModelEventRegistry.GetViewModelsByEvent(item))
            {
                try
                {
                    var isAsyncHandler = viewModel
                        .GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .Any(type =>type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IAsyncViewModelEventHandler<>));

                    var methodName = $"Handle{(isAsyncHandler ? "Async" : "")}";

                    var handler = viewModel.GetType().GetRuntimeMethod(methodName, new[] {item.Payload.GetType()});

                    var taskOrVoid = (Task)handler?.Invoke(viewModel, new object[] { item.Payload });
                    if (taskOrVoid != null) await taskOrVoid;
                }
                catch (Exception e)
                {
                    this.logger.Error("Exception during update view model", e);
                }
            }
        }
    }
}
