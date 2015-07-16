namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandService
    {
        void Execute(ICommand command, string origin = null, bool handleInBatch = false);
    }
}
