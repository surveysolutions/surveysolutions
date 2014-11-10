using System;
using System.Collections.Generic;
using System.Reflection;
using Ncqrs.Commanding.CommandExecution;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;

namespace Ncqrs.Commanding.ServiceModel
{
    // TODO: TLK, KP-4337: remove with all used stuff
    public class CommandService : ICommandService
    {
        protected readonly ILogger Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Dictionary<Type, Action<ICommand, string>> _executors = new Dictionary<Type, Action<ICommand, string>>();
        private readonly List<ICommandServiceInterceptor> _interceptors = new List<ICommandServiceInterceptor>(0);

        public virtual void Execute(ICommand command, string origin)
        {
            Type commandType = command.GetType();
            var context = new CommandContext(command);

            try
            {
                // Call OnBeforeExecution on every interceptor.
                foreach (var interceptor in _interceptors)
                {
                    interceptor.OnBeforeBeforeExecutorResolving(context);
                }

                // Get executor for the command.
                var executor = GetCommandExecutorForCommand(commandType);
                context.ExecutorResolved = executor != null;

                // Call OnBeforeExecution on every interceptor.
                foreach (var interceptor in _interceptors)
                {
                    interceptor.OnBeforeExecution(context);
                }

                // When we couldn't find an executor, throw exception.
                if (executor == null)
                {
                    throw new ExecutorForCommandNotFoundException(commandType);
                }

                // Set mark that the command executor has been called for this command.
                context.ExecutorHasBeenCalled = true;

                // Execute the command.
                executor(command, origin);
            }
            catch(Exception caught)
            {
                // There was an exception, add it to the context
                // and retrow.
                context.Exception = caught;
                throw;
            }
            finally
            {
                // Call OnAfterExecution on every interceptor.
                foreach (var interceptor in _interceptors)
                {
                    interceptor.OnAfterExecution(context);
                }
            }
        }

        public virtual void RegisterExecutor(Type commandType, ICommandExecutor<ICommand> executor)
        {
            RegisterExecutor<ICommand>(commandType, executor);
        }

        public virtual void RegisterExecutor<TCommand>(Type commandType, ICommandExecutor<TCommand> executor) where TCommand : ICommand
        {
            if (_executors.ContainsKey(commandType)) return;
            Action<ICommand, string> action = (cmd, origin) => executor.Execute((TCommand) cmd, origin);
            _executors.Add(commandType, action);
        }

        public virtual void RegisterExecutor<TCommand>(ICommandExecutor<TCommand> executor) where TCommand : ICommand
        {
            if (_executors.ContainsKey(typeof(TCommand))) return;
            Action<ICommand, string> action = (cmd, origin) => executor.Execute((TCommand)cmd, origin);
            _executors.Add(typeof(TCommand), action);
        }

        public virtual void UnregisterExecutor<TCommand>() where TCommand : ICommand
        {
            _executors.Remove(typeof (TCommand));
        }

        /// <summary>
        /// Gets the command executor for command.
        /// </summary>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>
        /// A command executor to use to execute the command or <c>null</c> if not found.
        /// </returns>
        protected virtual Action<ICommand, string> GetCommandExecutorForCommand(Type commandType)
        {
            Action<ICommand, string> result;
            _executors.TryGetValue(commandType, out result);

            return result;
        }

        /// <summary>
        /// Adds the interceptor. The interceptor will be called on every
        /// command execution.
        /// </summary>
        /// <remarks>
        /// When the interceptor was already added to this command service, it
        /// is skipped. That means that it is not added twice.
        /// </remarks>
        /// <param name="interceptor">The interceptor to add.</param>
        public virtual void AddInterceptor(ICommandServiceInterceptor interceptor)
        {
            if (!_interceptors.Contains(interceptor))
            {
                _interceptors.Add(interceptor);
            }
        }

        /// <summary>
        /// Removes the interceptor. The interceptor will not be called anymore.
        /// </summary>
        /// <param name="interceptor">The interceptor to remove.</param>
        public virtual void RemoveInterceptor(ICommandServiceInterceptor interceptor)
        {
            _interceptors.Remove(interceptor);
        }
    }
}
