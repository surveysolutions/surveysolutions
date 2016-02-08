using System;
using Main.Core.Documents;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class ImportFromDesigner : QuestionnaireCommand
    {
        public ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode, string supportingAssembly, long questionnaireContentVersion)
            : base(source.PublicKey, source.PublicKey)
        {
            AllowCensusMode = allowCensusMode;
            CreatedBy = createdBy;
            Source = source;
            SupportingAssembly = supportingAssembly;
            this.QuestionnaireContentVersion = questionnaireContentVersion;
        }

        public Guid CreatedBy { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public IQuestionnaireDocument Source { get; private set; }
        public string SupportingAssembly { get; private set; }
        public long QuestionnaireContentVersion { get; private set; }
    }
}
