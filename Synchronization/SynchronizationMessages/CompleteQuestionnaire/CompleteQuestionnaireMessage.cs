using System;
using RavenQuestionnaire.Core.Commands;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class EventSyncMessage
    {
        public Guid SynchronizationKey { get; set; }
        public ICommand Command { get; set; }
    }
}
