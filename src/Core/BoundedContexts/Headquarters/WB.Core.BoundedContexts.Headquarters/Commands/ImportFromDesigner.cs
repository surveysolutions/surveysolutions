using System;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Commands
{
    public class ImportFromDesigner : QuestionnaireCommand
    {
        public ImportFromDesigner(Guid createdBy, IQuestionnaireDocument source, bool allowCensusMode,
            string supportingAssembly, long questionnaireContentVersion, long questionnaireVersion, string comment,
            bool? criticalitySupport, CriticalityLevel? criticalityLevel)
            : base(source.PublicKey, source.PublicKey)
        {
            this.AllowCensusMode = allowCensusMode;
            this.CreatedBy = createdBy;
            this.Source = source;
            this.SupportingAssembly = supportingAssembly;
            this.QuestionnaireContentVersion = questionnaireContentVersion;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Comment = comment;
            this.CriticalitySupport = criticalitySupport;
            this.CriticalityLevel = criticalityLevel;
        }

        public bool? CriticalitySupport { get; set; }

        public CriticalityLevel? CriticalityLevel { set; get; }

        public string Comment { get; private set; }

        public Guid CreatedBy { get; private set; }
        public bool AllowCensusMode { get; private set; }
        public IQuestionnaireDocument Source { get; private set; }
        public string SupportingAssembly { get; private set; }
        public long QuestionnaireContentVersion { get; private set; }
        public long QuestionnaireVersion { get; private set; }
    }
}
