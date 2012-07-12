using System;
using Ncqrs.Eventing.Storage;

namespace RavenQuestionnaire.Core.Events.Questionnaire.Completed
{
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:QuestionnaireLoaded")]
    public class QuestionnaireLoaded
    {

    }
}
