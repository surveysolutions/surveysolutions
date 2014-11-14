using System;
using Main.Core.Documents;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Questionnaire), "ImportFromDesigner")]
    public class ImportFromDesigner : QuestionnaireCommand
    {
        public ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode, string supportingAssembly)
            : base(source.PublicKey, source.PublicKey)
        {
            this.AllowCensusMode = allowCensusMode;
            CreatedBy = createdBy;
            Source = source;
            this.SupportingAssembly = supportingAssembly;
        }

        public Guid CreatedBy { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public IQuestionnaireDocument Source { get; private set; }
        public string SupportingAssembly { get; private set; }
    }
}
