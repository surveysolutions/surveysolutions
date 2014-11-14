using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Questionnaire), "ImportFromDesignerForTester")]
    public class ImportFromDesignerForTester : QuestionnaireCommand
    {
        public ImportFromDesignerForTester(IQuestionnaireDocument source)
            : base(source.PublicKey, source.PublicKey)
        {
            Source = source;
        }

        public IQuestionnaireDocument Source { get; private set; }
    }
}
