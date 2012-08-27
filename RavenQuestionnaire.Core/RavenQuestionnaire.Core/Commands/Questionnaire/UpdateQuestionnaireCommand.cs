using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateQuestionnaire")]
    public class UpdateQuestionnaireCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public string Title{get; set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionnaireId"></param>
        /// <param name="title"></param>
        public UpdateQuestionnaireCommand(Guid questionnaireId, string title)
        {
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
        }

        
    }
}
