using System;
using System.IO;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Commands;
using SynchronizationMessages.Synchronization;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class EventSyncMessage : ICustomSerializable
    {
        public Guid SynchronizationKey { get; set; }
        public Guid CommandKey { get; set; }
        public ICommand Command { get; set; }


        public void WriteTo(Stream stream)
        {
            FormatHelper.WriteGuid(stream, this.SynchronizationKey);
            FormatHelper.WriteGuid(stream, this.CommandKey);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            var command= JsonConvert.SerializeObject(Command, Formatting.Indented, settings);
            FormatHelper.WriteString(stream, command);
        }

        public void InitializeFrom(Stream stream)
        {
            this.SynchronizationKey = FormatHelper.ReadGuid(stream);
            this.CommandKey = FormatHelper.ReadGuid(stream);
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            var commandString = FormatHelper.ReadString(stream);
            var command = JsonConvert.DeserializeObject<ICommand>(commandString, settings);
          
            this.Command = command;
        }
    }
}
