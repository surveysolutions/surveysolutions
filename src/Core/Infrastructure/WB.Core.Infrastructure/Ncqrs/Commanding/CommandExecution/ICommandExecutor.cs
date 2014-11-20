using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Commanding.CommandExecution
{
    /// <summary>
    /// Executes a command. This means that the handles 
    /// executes the correct action based on the command.
    /// </summary>
    public interface ICommandExecutor<in TCommand> where TCommand : ICommand
    {
        void Execute(TCommand command, string origin);
    }
}
