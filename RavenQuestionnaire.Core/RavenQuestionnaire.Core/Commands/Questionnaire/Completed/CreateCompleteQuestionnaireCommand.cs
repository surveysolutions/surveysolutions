using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(CompleteQuestionnaireAR))]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        public CreateCompleteQuestionnaireCommand(){}

        public Guid CompletedQuestionnaireId
        {
            get; set;
        }

        public Guid QuestionnaireId
        {
            get;
            set;
        }

        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, Guid questionnaireId)
            : base(completedQuestionnaireId)
        {
            CompletedQuestionnaireId = completedQuestionnaireId;
            QuestionnaireId = questionnaireId;
        }


    }
}
