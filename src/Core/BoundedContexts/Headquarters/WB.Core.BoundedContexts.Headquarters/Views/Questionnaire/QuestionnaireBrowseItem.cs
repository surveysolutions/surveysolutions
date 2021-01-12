using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class QuestionnaireBrowseItem
    {
        public QuestionnaireBrowseItem()
        {
            this.FeaturedQuestions = new List<FeaturedQuestionItem>();
        }

        public QuestionnaireBrowseItem(QuestionnaireDocument doc, long version, bool allowCensusMode,
            long questionnaireContentVersion, bool isSupportAssignments, bool allowExportVariables, string comment,
            Guid? importedBy)
        {
            this.FeaturedQuestions =
                doc.Find<IQuestion>(q => q.Featured)
                    .Select(q => new FeaturedQuestionItem(q.PublicKey, q.QuestionText, q.StataExportCaption))
                    .ToList();
            this.QuestionnaireContentVersion = questionnaireContentVersion;
            this.Id = string.Format("{0}${1}", doc.PublicKey.FormatGuid(), version);
            this.Comment = comment;
            this.QuestionnaireId = doc.PublicKey;
            this.Version = version;
            this.Title = doc.Title;
            this.Variable = doc.VariableName;
            this.CreationDate = doc.CreationDate;
            this.LastEntryDate = doc.LastEntryDate;
            this.CreatedBy = doc.CreatedBy;
            this.IsPublic = doc.IsPublic;
            this.AllowCensusMode = allowCensusMode;
            this.AllowAssignments = isSupportAssignments;
            this.AllowExportVariables = allowExportVariables;
            this.ImportDate = DateTime.UtcNow;
            this.ImportedBy = importedBy;
        }

        public virtual string Comment { get; set; }

        public virtual string Id { get; set; }

        public virtual DateTime CreationDate { get;  set; }

        public virtual Guid QuestionnaireId { get;  set; }

        public virtual long Version { get; set; }

        public virtual DateTime LastEntryDate { get;  set; }

        public virtual string Title { get;  set; }

        public virtual string Variable { get;  set; }

        public virtual bool IsPublic { get; set; }

        public virtual Guid? CreatedBy { get;  set; }

        public virtual bool IsDeleted { get; set; }

        public virtual bool AllowCensusMode { get; set; }

        public virtual bool Disabled { get; set; }

        public virtual Guid? DisabledBy { get; set; }

        public virtual long QuestionnaireContentVersion { get; set; }

        public virtual IList<FeaturedQuestionItem> FeaturedQuestions { get; protected set; }

        public virtual DateTime? ImportDate { get; set; }
        public virtual Guid? ImportedBy { get; set; }

        public virtual bool AllowAssignments { get; set; }

        public virtual bool AllowExportVariables { get; set; }

        public virtual QuestionnaireIdentity Identity() => new QuestionnaireIdentity(QuestionnaireId, Version);

        public virtual bool IsAudioRecordingEnabled { get; set; }
        public virtual bool WebModeEnabled { get; set; }
    }
}
