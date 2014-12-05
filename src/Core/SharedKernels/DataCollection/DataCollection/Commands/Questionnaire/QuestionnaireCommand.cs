using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public abstract class QuestionnaireCommand : CommandBase
    {
        protected QuestionnaireCommand(Guid commandIdentifier, Guid questionnaireId)
            : base(commandIdentifier)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}