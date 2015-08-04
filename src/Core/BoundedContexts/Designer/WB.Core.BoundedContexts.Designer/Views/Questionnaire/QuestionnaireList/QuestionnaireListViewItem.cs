using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListViewItem : IView
    {
        public QuestionnaireListViewItem()
        {
            this.SharedPersons =new HashSet<Guid>();
        }

        public QuestionnaireListViewItem(Guid id, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy, bool isPublic) : this()
        {
            this.PublicId = id;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
        }

        public virtual DateTime CreationDate { get; set; }

        public virtual string QuestionnaireId { get; set; }

        public virtual Guid PublicId
        {
            get { return publicId; }
            set
            {
                this.QuestionnaireId = value.FormatGuid();
                publicId = value;
            }
        }

        private Guid publicId;

        public virtual DateTime LastEntryDate { get; set; }

        public virtual string Title { get; set; }

        public virtual Guid? CreatedBy { get; set; }

        public virtual string CreatorName { get; set; }

        public virtual bool IsDeleted { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual ISet<Guid> SharedPersons { get; set; }

        public virtual string Owner { get; set; }
    }
}