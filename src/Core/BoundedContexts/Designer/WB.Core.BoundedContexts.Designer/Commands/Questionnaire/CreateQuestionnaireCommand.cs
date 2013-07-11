using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(QuestionnaireAR))]
    public class CreateQuestionnaireCommand : CommandBase
    {
        public CreateQuestionnaireCommand()
        {
        }

        public CreateQuestionnaireCommand(Guid questionnaireId, string text, Guid? createdBy = null, bool isPublic = false)
            : base(questionnaireId)
        {
            this.PublicKey = questionnaireId;
            this.Title = text;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? CreatedBy { get; set; }

        public bool IsPublic { get; set; }
    }
}