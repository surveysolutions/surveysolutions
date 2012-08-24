using System;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireTemplateLoaded")]
    public class QuestionnaireTemplateLoaded
    {
        public QuestionnaireDocument Template { get; set; }
    }
}
