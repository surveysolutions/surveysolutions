using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Events.Questionnaire
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireTemplateLocaded")]
    public class QuestionnaireTemplateLocaded
    {
        public QuestionnaireDocument Template { get; set; }
    }
}
