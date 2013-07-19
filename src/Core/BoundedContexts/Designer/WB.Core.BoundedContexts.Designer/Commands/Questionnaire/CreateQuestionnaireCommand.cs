using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(Aggregates.Questionnaire))]
    public class CreateQuestionnaireCommand : CommandBase
    {
        public CreateQuestionnaireCommand(Guid questionnaireId, string text, Guid? createdBy = null, bool isPublic = false)
            : base(questionnaireId)
        {
            this.PublicKey = questionnaireId;
            this.Title = text;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        public Guid PublicKey { get; private set; }

        public string Title { get; private set; }

        public Guid? CreatedBy { get; private set; }

        public bool IsPublic { get; private set; }
    }
}