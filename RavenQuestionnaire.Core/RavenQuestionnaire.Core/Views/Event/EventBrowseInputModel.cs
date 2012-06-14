using System;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventBrowseInputModel
    {
        public EventBrowseInputModel(Guid? publicKey)
        {
            PublickKey = publicKey;
        }

        public Guid? PublickKey { get; private set; }
    }
}
