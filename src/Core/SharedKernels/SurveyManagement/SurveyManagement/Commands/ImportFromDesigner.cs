using System;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.SurveyManagement.Commands
{
    public class ImportFromDesigner : QuestionnaireCommand
    {
        public ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode, string supportingAssembly, long questionnaireContentVersion)
            : base(source.PublicKey, source.PublicKey)
        {
            this.AllowCensusMode = allowCensusMode;
            this.CreatedBy = createdBy;
            this.Source = source;
            this.SupportingAssembly = supportingAssembly;
            this.QuestionnaireContentVersion = questionnaireContentVersion;
        }

        public Guid CreatedBy { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public IQuestionnaireDocument Source { get; private set; }
        public string SupportingAssembly { get; private set; }
        public long QuestionnaireContentVersion { get; private set; }
    }
}
