using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireItemMoved")]
    public class QuestionnaireItemMoved
    {
        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid? GroupKey { get; set; }
        public Guid? AfterItemKey { get; set; }
    }
}
