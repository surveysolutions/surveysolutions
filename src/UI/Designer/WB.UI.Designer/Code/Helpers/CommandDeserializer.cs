namespace WB.UI.Designer.Code.Helpers
{
    using System;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;

    using Newtonsoft.Json.Linq;

    internal class CommandDeserializer : ICommandDeserializer
    {
        public ICommand Deserialize(string serializedCommand)
        {
            dynamic parsedCommand = JObject.Parse(serializedCommand);

            ICommand concreteCommand = new NewUpdateGroupCommand(
                Guid.Parse((string)parsedCommand.questionnaireId),
                Guid.Parse((string)parsedCommand.groupId),
                (string)parsedCommand.title);

            return concreteCommand;
        }
    }
}