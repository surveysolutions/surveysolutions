using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CreateQuestionnaireCommand : CommandBase
    {
        public Guid QuestionnaireId
        {
            get; set;
        }

        public String Text
        {
            get; set;
        }

        public CreateQuestionnaireCommand()
        {
        }

        public CreateQuestionnaireCommand(Guid questionnaireId, string text): base (questionnaireId)
        {
            QuestionnaireId = questionnaireId;
            Text = text;
        }

    }
}
