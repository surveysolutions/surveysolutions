namespace WB.UI.Designer.Code.Helpers
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Commands.Questionnaire.Group;
    using Main.Core.Commands.Questionnaire.Question;

    using Ncqrs.Commanding;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class CommandDeserializer : ICommandDeserializer
    {
        private static readonly Dictionary<string, Type> knownCommandTypes = new Dictionary<string, Type>
        {
            { "UpdateGroup", typeof(NewUpdateGroupCommand) },
            { "AddGroup", typeof(NewAddGroupCommand) },
            { "DeleteGroup", typeof(NewDeleteGroupCommand) },
            { "UpdateQuestion", typeof(NewUpdateQuestionCommand) },
            { "AddQuestion", typeof(NewAddQuestionCommand) },
            { "DeleteQuestion", typeof(NewDeleteQuestionCommand) },
        };

        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);

            return (ICommand) JsonConvert.DeserializeObject(serializedCommand, resultCommandType);
        }

        private static Type GetTypeOfResultCommandOrThrowArgumentException(string commandType)
        {
            if (!knownCommandTypes.ContainsKey(commandType))
                throw new CommandDeserializationException(string.Format("Command type '{0}' is not supported.", commandType));

            return knownCommandTypes[commandType];
        }
    }
}