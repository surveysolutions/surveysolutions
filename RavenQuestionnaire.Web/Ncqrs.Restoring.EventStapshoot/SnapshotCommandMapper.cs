using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding.CommandExecution.Mapping;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using Ncqrs.Domain;

namespace Ncqrs.Restoring.EventStapshoot
{
    public class SnapshotCommandMapper : ICommandMapper
    {
        public void Map(ICommand command, IMappedCommandExecutor executor)
        {
            var typed = command as CreateSnapshotForAR;
            if(typed==null)
                return;

            
            var commandType = command.GetType();

            var match = GetMatchingMethod(typed.AggregateRootType, commandType, "CreateNewSnapshot");

            Action<AggregateRoot, ICommand> existingAction =
                (agg, cmd) =>
                {
                    var parameter = match.Item2.Select(p => p.GetValue(cmd, null));
                    match.Item1.Invoke(agg, parameter.ToArray());
                };

            var creatingMatch = GetMatchingConstructor(typed.AggregateRootType, commandType);
            Func<ICommand, AggregateRoot> create = (c) =>
            {
                var parameter = match.Item2.Select(p => p.GetValue(c, null));
                return (AggregateRoot)creatingMatch.Item1.Invoke(parameter.ToArray());
            };

            Action executorAction = () => executor.ExecuteActionOnExistingOrCreatingNewInstance((c)=>typed.AggregateRootId,(c)=> typed.AggregateRootType, existingAction, create);

            if (commandType.IsDefined(typeof(TransactionalAttribute), false))
            {
                var transactionService = NcqrsEnvironment.Get<ITransactionService>();
                transactionService.ExecuteInTransaction(executorAction);
            }
            else
            {
                executorAction();
            }
        }
        private  Tuple<ConstructorInfo, PropertyInfo[]> GetMatchingConstructor(Type arType, Type commandType)
        {
          /*  var strategy = new AttributePropertyMappingStrategy();
            var sources = strategy.GetMappedProperties(commandType);*/

            return PropertiesToMethodMapper.GetConstructor(new PropertyToParameterMappingInfo[0], arType);
        }
        private static Tuple<MethodInfo, PropertyInfo[]> GetMatchingMethod(Type arType, Type commandType, string methodName)
        {
         /*   var strategy = new AttributePropertyMappingStrategy();
            var sources = strategy.GetMappedProperties(commandType);*/

            return PropertiesToMethodMapper.GetMethod(new PropertyToParameterMappingInfo[0], arType, methodName);
        }
    }
}
