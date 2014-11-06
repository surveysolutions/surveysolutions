namespace WB.Core.Infrastructure.CommandBus
{
    public class CommandService : ICommandService
    {
        private readonly ICommandService ncqrsCommandService;

        public CommandService(ICommandService ncqrsCommandService)
        {
            this.ncqrsCommandService = ncqrsCommandService;
        }

        public void Execute(ICommand command, string origin)
        {
            this.FallbackToNcqrs(command, origin);
        }

        private void FallbackToNcqrs(ICommand command, string origin)
        {
            this.ncqrsCommandService.Execute(command, origin);
        }
    }
}