using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateQuestionnaire")]
    public class UpdateQuestionnaireCommand : CommandBase
    {
        [AggregateRootId]
        public string QuestionnaireId { get; set; }

        public string Title{get; set;}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionnaireId"></param>
        /// <param name="title"></param>
        public UpdateQuestionnaireCommand(string questionnaireId, string title)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Title = title;
        }

        
    }
}
