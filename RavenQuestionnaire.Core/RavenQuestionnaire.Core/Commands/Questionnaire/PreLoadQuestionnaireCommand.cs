using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "PreLoad")]
    public class PreLoadQuestionnaireCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
    }
}
