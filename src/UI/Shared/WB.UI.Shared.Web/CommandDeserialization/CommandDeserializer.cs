using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

using System;
using System.Collections.Generic;
using Ncqrs.Commanding;
using Newtonsoft.Json;

namespace WB.UI.Shared.Web.CommandDeserialization
{
    internal abstract class CommandDeserializer : ICommandDeserializer
    {
        protected abstract Dictionary<string, Type> KnownCommandTypes { get; }
        
        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);

            ICommand command = null;
            try
            {
                command = (ICommand)JsonConvert.DeserializeObject(serializedCommand, resultCommandType);
            }
            catch
            {
                throw new CommandDeserializationException(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", commandType, serializedCommand));
            }
            
            return command;
        }

        private Type GetTypeOfResultCommandOrThrowArgumentException(string commandType)
        {
            if (!KnownCommandTypes.ContainsKey(commandType))
                throw new CommandDeserializationException(string.Format("Command type '{0}' is not supported.", commandType));

            return KnownCommandTypes[commandType];
        }
    }
}