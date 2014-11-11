using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class ImportFromSupervisor : CommandBase
    {
        public ImportFromSupervisor(IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            Source = source;
            QuestionnaireId = source.PublicKey;
        }

        public IQuestionnaireDocument Source { get; private set; }

        public Guid QuestionnaireId { get; private set; }
    }
}
