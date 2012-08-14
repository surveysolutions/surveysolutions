using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RavenQuestionnaire.Core.Documents;
using SynchronizationMessages.Synchronization;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class ListOfAggregateRootsForImportMessage : ICustomSerializable
    {
        public IList<ProcessedEventChunk> Roots { get; set; }

        #region Implementation of ICustomSerializable

        public void WriteTo(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;

            var rootsString = JsonConvert.SerializeObject(Roots, Formatting.Indented, settings);
            FormatHelper.WriteString(stream, rootsString);
        }

        public void InitializeFrom(Stream stream)
        {
            var settings = new JsonSerializerSettings();
            settings.TypeNameHandling = TypeNameHandling.Objects;
            var rootsString = FormatHelper.ReadString(stream);
            this.Roots = JsonConvert.DeserializeObject<IList<ProcessedEventChunk>>(rootsString, settings);
        }

        #endregion
    }
}
