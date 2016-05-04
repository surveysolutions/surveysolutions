using System;

namespace WB.Core.SharedKernels.SurveyManagement.Commands
{
    public class RegisterPlainQuestionnaire : QuestionnaireCommand
    {
        public Guid Id { get; private set; }
        public long Version { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public string SupportingAssembly { get; private set; }

        public RegisterPlainQuestionnaire(Guid questionnaireId, long version, bool allowCensusMode, string supportingAssembly)
            : base(questionnaireId, questionnaireId)
        {
            this.AllowCensusMode = allowCensusMode;
            this.Id = questionnaireId;
            this.Version = version;
            this.SupportingAssembly = supportingAssembly;
        }
    }
}