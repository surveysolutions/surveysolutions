using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class RegisterPlainQuestionnaire : CommandBase
    {
        public Guid QuestionnaireId { get; private set; }

        public Guid Id { get; private set; }
        public long Version { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public string SupportingAssembly { get; private set; }

        public RegisterPlainQuestionnaire(Guid questionnaireId, long version, bool allowCensusMode, string supportingAssembly)
            : base(questionnaireId)
        {
            this.AllowCensusMode = allowCensusMode;
            this.QuestionnaireId = questionnaireId;
            this.Id = questionnaireId;
            this.Version = version;
            this.SupportingAssembly = supportingAssembly;
        }
    }
}