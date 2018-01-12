using System.Threading;
using System.Threading.Tasks;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterCommandService : ICommandService
    {
        private readonly IAppdomainsPerInterviewManager interviews;
        private readonly ILiteEventBus eventBus;

        public WebTesterCommandService(IAppdomainsPerInterviewManager interviews,
            ILiteEventBus eventBus)
        {
            this.interviews = interviews;
            this.eventBus = eventBus;
        }

        public void Execute(ICommand command, string origin = null)
        {
            var events = this.interviews.Execute(command);
            eventBus.PublishCommittedEvents(events);
        }

        public Task ExecuteAsync(ICommand command, string origin = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }

        public Task WaitPendingCommandsAsync() => Task.CompletedTask;

        public bool HasPendingCommands => false;
    }
}