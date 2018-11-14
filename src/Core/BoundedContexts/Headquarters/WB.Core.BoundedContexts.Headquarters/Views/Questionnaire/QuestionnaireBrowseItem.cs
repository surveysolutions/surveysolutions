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

        protected QuestionnaireBrowseItem(Guid questionnaireId, 
            long version, 
            string title, 
            string variable,
            DateTime creationDate, 
            DateTime lastEntryDate, 
            Guid? createdBy, 
            bool isPublic, 
            bool allowCensusMode,
            bool allowAssignments,
            bool allowExportVariables)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Title = title;
            this.Variable = variable;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
            this.AllowCensusMode = allowCensusMode;
            this.AllowAssignments = allowAssignments;
            this.AllowExportVariables = allowExportVariables;
            this.ImportDate = DateTime.UtcNow;
        }

        public QuestionnaireBrowseItem(QuestionnaireDocument doc, long version, bool allowCensusMode, long questionnaireContentVersion, bool isSupportAssignments, bool allowExportVariables)
            : this(doc.PublicKey, version, doc.Title, doc.VariableName, doc.CreationDate, doc.LastEntryDate, doc.CreatedBy, doc.IsPublic, allowCensusMode, isSupportAssignments, allowExportVariables)
        {
            this.FeaturedQuestions =
                doc.Find<IQuestion>(q => q.Featured)
                   .Select(q => new FeaturedQuestionItem(q.PublicKey, q.QuestionText, q.StataExportCaption))
                   .ToList();
            this.QuestionnaireContentVersion = questionnaireContentVersion;
            this.Id = string.Format("{0}${1}", doc.PublicKey.FormatGuid(), version);
        }

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

        public virtual bool AllowAssignments { get; set; }

        public virtual bool AllowExportVariables { get; set; }

        public virtual QuestionnaireIdentity Identity() => new QuestionnaireIdentity(QuestionnaireId, Version);
    }
}
