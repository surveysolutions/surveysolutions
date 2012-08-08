using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Events;
using SynchronizationMessages.Synchronization;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class ImportSynchronizationMessage : ICustomSerializable
    {
        public AggregateRootEventStream EventStream { get; set; }

        #region Implementation of ICustomSerializable

        public void WriteTo(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            var command = JsonConvert.SerializeObject(EventStream, Formatting.Indented, settings);
            FormatHelper.WriteString(stream, command);
        }

        public void InitializeFrom(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            var commandString = FormatHelper.ReadString(stream);
            var command = JsonConvert.DeserializeObject<AggregateRootEventStream>(commandString, settings);

            this.EventStream = command;
        }

        #endregion
    }
}
