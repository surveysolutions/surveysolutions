using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireBrowseItem : IView
    {
        public QuestionnaireBrowseItem()
        {
        }

        protected QuestionnaireBrowseItem(Guid questionnaireId, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy, bool isPublic)
        {
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        public QuestionnaireBrowseItem(QuestionnaireDocument doc)
            : this(doc.PublicKey, doc.Title, doc.CreationDate, doc.LastEntryDate, doc.CreatedBy, doc.IsPublic)
        {
            this.FeaturedQuestions =
                doc.Find<IQuestion>(q => q.Featured)
                   .Select(q => new FeaturedQuestionItem(q.PublicKey, q.QuestionText, q.StataExportCaption))
                   .ToArray();
        }

        public DateTime CreationDate { get;  set; }

        public Guid QuestionnaireId { get;  set; }

        public DateTime LastEntryDate { get;  set; }

        public string Title { get;  set; }

        public bool IsPublic { get; set; }

        public Guid? CreatedBy { get;  set; }

        public bool IsDeleted { get; set; }

        public FeaturedQuestionItem[] FeaturedQuestions { get;  set; }

    }
}