using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Questionnaire), "ImportFromDesigner")]
    public class ImportFromDesigner : CommandBase
    {
        public ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode)
            : base(source.PublicKey)
        {
            this.AllowCensusMode = allowCensusMode;
            CreatedBy = createdBy;
            Source = source;
            QuestionnaireId = source.PublicKey;
        }

        public Guid CreatedBy { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public IQuestionnaireDocument Source { get; private set; }

        [AggregateRootId]
        public Guid QuestionnaireId { get; private set; }
    }
}
