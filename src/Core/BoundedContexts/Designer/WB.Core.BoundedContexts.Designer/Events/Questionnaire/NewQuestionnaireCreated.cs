using System;
using Ncqrs.Eventing.Storage;

namespace Main.Core.Events.Questionnaire
{
    [EventName("RavenQuestionnaire.Core:Events:NewQuestionnaireCreated")]
    public class NewQuestionnaireCreated
    {
        public DateTime CreationDate { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }
    }
}