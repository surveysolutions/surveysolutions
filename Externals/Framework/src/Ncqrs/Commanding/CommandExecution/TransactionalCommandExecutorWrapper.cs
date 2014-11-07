using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Commanding.CommandExecution
{
    /// <summary>
    /// Wraps transactional behavior around the execution of command executor.
    /// </summary>
    /// <remarks>
    /// The transaction logic uses <c>TransactionScope</c> of the .NET framework.
    /// </remarks>
    public class TransactionalCommandExecutorWrapper<TCommand> : ICommandExecutor<TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// The executor to use to execute the command.
        /// </summary>
        private readonly ICommandExecutor<TCommand> _executor;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalCommandExecutorWrapper{TCommand}"/> class.
        /// </summary>
        /// <param name="executor">The executor to use to execute the command.</param>
        public TransactionalCommandExecutorWrapper(ICommandExecutor<TCommand> executor)
        {
            _executor = executor;
        }
        
        public void Execute(TCommand command, string origin)
        {
            var transactionService = NcqrsEnvironment.Get<ITransactionService>();
            transactionService.ExecuteInTransaction(() => _executor.Execute(command, origin));
        }
    }
}
