using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class ImportFromSupervisor : QuestionnaireCommand
    {
        public ImportFromSupervisor(IQuestionnaireDocument source)
            : base(source.PublicKey, source.PublicKey)
        {
            Source = source;
        }

        public IQuestionnaireDocument Source { get; private set; }
    }
}
