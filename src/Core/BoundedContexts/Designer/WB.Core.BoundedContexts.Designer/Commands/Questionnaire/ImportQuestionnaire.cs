using System;
using Main.Core.Documents;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class ImportQuestionnaire : CommandBase
    {
        public ImportQuestionnaire(Guid createdBy, IQuestionnaireDocument source)
            : base(source.PublicKey)
        {
            CreatedBy = createdBy;
            Source = source;
            QuestionnaireId = source.PublicKey;
        }

        public Guid CreatedBy { get; private set; }

        public IQuestionnaireDocument Source { get; private set; }

        public Guid QuestionnaireId { get; private set; }
    }
}
