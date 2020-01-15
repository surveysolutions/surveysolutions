using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Shared.Web.CommandDeserialization
{
    public abstract class CommandDeserializer : ICommandDeserializer
    {
        protected abstract Dictionary<string, Type> KnownCommandTypes { get; }
        private readonly ILogger logger;

        protected CommandDeserializer(ILogger logger)
        {
            this.logger = logger;
        }

        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            try
            {
                Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);
                ICommand command = (ICommand)JsonConvert.DeserializeObject(serializedCommand, resultCommandType);
                
                return command;
            }
            catch(Exception e)
            {
                logger.Error("Error on command deserialization.", e);
                throw new CommandDeserializationException(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", commandType, serializedCommand));
            }
        }

        private Type GetTypeOfResultCommandOrThrowArgumentException(string commandType)
        {
            if (!KnownCommandTypes.ContainsKey(commandType))
                throw new CommandDeserializationException(string.Format("Command type '{0}' is not supported.", commandType));

            return KnownCommandTypes[commandType];
        }
    }
}
