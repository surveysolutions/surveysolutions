using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;

namespace WB.UI.Shared.Web.CommandDeserialization
{
    public abstract class CommandDeserializer : ICommandDeserializer
    {
        protected abstract Dictionary<string, Type> KnownCommandTypes { get; }
        private readonly ILogger logger;


        protected CommandDeserializer()
        {
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
        }


        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);

            ICommand command = null;
            try
            {
                command = (ICommand)JsonConvert.DeserializeObject(serializedCommand, resultCommandType);
            }
            catch(Exception e)
            {
                logger.Error("Error on command deserialization.", e);
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