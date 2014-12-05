using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
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
