using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class RegisterPlainQuestionnaire : CommandBase
    {
        public Guid Id { get; private set; }
        public long Version { get; private set; }

        public RegisterPlainQuestionnaire(Guid questionnaireId, long version)
            : base(questionnaireId)
        {
            this.Id = questionnaireId;
            this.Version = version;
        }
    }
}