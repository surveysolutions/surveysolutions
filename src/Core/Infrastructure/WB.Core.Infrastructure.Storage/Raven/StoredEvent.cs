﻿using System;
using System.Diagnostics;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Converters;

namespace WB.Core.Infrastructure.Storage.Raven
{
    [DebuggerDisplay("StoredEvent {Data.GetType().Name}, Id = {EventIdentifier}, EventTimeStamp = {EventTimeStamp}, EventSequence = {EventSequence}")]
    public class StoredEvent
    {
        public string Id { get; set; }
        public long EventSequence { get; set; }
        public Guid EventSourceId { get; set; }
        public Guid CommitId { get; set; }
        public string Origin { get; set; }
        public Guid EventIdentifier { get; set; }
        public DateTime EventTimeStamp { get; set; }
        
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Data { get; set; }

        public string EventType { get; set; }
    }
}