using System;
using NServiceBus;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class CompleteQuestionnaireMessage: IMessage
    {
        public Guid SynchronizationKey { get; set; }
        public string Questionanire { get; set; }
    }
}
