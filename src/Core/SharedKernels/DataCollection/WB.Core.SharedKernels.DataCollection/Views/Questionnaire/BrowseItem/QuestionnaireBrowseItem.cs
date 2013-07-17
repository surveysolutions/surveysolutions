using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.View.Questionnaire
{
    using System;

    using Main.Core.Documents;
    using Main.Core.Entities;

    public class QuestionnaireBrowseItem : IView
    {
        public QuestionnaireBrowseItem()
        {
        }

        public QuestionnaireBrowseItem(Guid questionnaireId, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy, bool isPublic)
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
        }

        public DateTime CreationDate { get; private set; }

        public Guid QuestionnaireId { get; private set; }

        public DateTime LastEntryDate { get; private set; }

        public string Title { get;  set; }

        public bool IsPublic { get; set; }

        public Guid? CreatedBy { get; private set; }

        public bool IsDeleted { get; set; }

        public static QuestionnaireBrowseItem New()
        {
            return new QuestionnaireBrowseItem(Guid.Empty, null, DateTime.Now, DateTime.Now, null, false);
        }

        public TemplateLight GetTemplateLight()
        {
            return new TemplateLight(this.QuestionnaireId, this.Title);
        }
    }
}