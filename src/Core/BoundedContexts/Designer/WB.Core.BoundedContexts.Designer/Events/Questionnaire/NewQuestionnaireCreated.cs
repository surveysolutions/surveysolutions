using System;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus.Lite;

namespace Main.Core.Events.Questionnaire
{
    public class NewQuestionnaireCreated : ILiteEvent
    {
        public DateTime CreationDate { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }
    }
}