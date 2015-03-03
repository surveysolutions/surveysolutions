using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireBrowseItem : IReadSideRepositoryEntity
    {
        public QuestionnaireBrowseItem()
        {
        }

        protected QuestionnaireBrowseItem(Guid questionnaireId, long version, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy, bool isPublic, bool allowCensusMode)
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
                   .ToArray();
        }

        public DateTime CreationDate { get;  set; }

        public Guid QuestionnaireId { get;  set; }

        public long Version { get; set; }

        public DateTime LastEntryDate { get;  set; }

        public string Title { get;  set; }

        public bool IsPublic { get; set; }

        public Guid? CreatedBy { get;  set; }

        public bool IsDeleted { get; set; }

        public FeaturedQuestionItem[] FeaturedQuestions { get;  set; }

        public bool AllowCensusMode { get; set; }

        public bool Disabled { get; set; }
    }
}