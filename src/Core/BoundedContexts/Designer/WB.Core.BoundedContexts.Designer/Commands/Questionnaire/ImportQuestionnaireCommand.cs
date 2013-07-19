using System;
using Main.Core.Documents;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Aggregates.Questionnaire), "ImportQuestionnaire")]
    public class ImportQuestionnaireCommand : CommandBase
    {
        public ImportQuestionnaireCommand(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            CreatedBy = createdBy;
            Source = source;
            QuestionnaireId = source.PublicKey;
        }

        public Guid CreatedBy { get; private set; }

        public IQuestionnaireDocument Source { get; private set; }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}
