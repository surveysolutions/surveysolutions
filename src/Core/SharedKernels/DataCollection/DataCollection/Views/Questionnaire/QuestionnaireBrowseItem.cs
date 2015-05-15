using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireBrowseItem : IReadSideRepositoryEntity
    {
        public QuestionnaireBrowseItem()
        {
            FeaturedQuestions = new List<FeaturedQuestionItem>();
        }

        protected QuestionnaireBrowseItem(Guid questionnaireId, 
            long version, 
            string title, 
            DateTime creationDate, 
            DateTime lastEntryDate, 
            Guid? createdBy, 
            bool isPublic, 
            bool allowCensusMode)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
            this.AllowCensusMode = allowCensusMode;
        }

        public QuestionnaireBrowseItem(QuestionnaireDocument doc, long version, bool allowCensusMode)
            : this(doc.PublicKey, version, doc.Title, doc.CreationDate, doc.LastEntryDate, doc.CreatedBy, doc.IsPublic, allowCensusMode)
        {
            this.FeaturedQuestions =
                doc.Find<IQuestion>(q => q.Featured)
                   .Select(q => new FeaturedQuestionItem(q.PublicKey, q.QuestionText, q.StataExportCaption))
                   .ToList();

            this.Id = string.Format("{0}${1}", doc.PublicKey.FormatGuid(), version);
        }

        public virtual string Id { get; set; }

        public virtual DateTime CreationDate { get;  set; }

        public virtual Guid QuestionnaireId { get;  set; }

        public virtual long Version { get; set; }

        public virtual DateTime LastEntryDate { get;  set; }

        public virtual string Title { get;  set; }

        public virtual bool IsPublic { get; set; }

        public virtual Guid? CreatedBy { get;  set; }

        public virtual bool IsDeleted { get; set; }

        public virtual bool AllowCensusMode { get; set; }

        public virtual bool Disabled { get; set; }

        [IgnoreDataMember]
        public virtual IList<FeaturedQuestionItem> FeaturedQuestions { get; protected set; }
    }
}