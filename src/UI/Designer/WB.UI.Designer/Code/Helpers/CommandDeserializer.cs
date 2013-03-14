namespace WB.UI.Designer.Code.Helpers
{
    using System;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    internal class CommandDeserializer : ICommandDeserializer
    {
        public ICommand Deserialize(string serializedCommand)
        {
            return JsonConvert.DeserializeObject<NewUpdateGroupCommand>(serializedCommand);
        }
    }
}