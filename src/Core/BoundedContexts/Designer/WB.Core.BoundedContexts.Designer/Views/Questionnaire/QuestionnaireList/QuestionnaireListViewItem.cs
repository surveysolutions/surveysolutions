using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListViewItem : IView
    {
        public QuestionnaireListViewItem()
        {
            this.SharedPersons = new List<Guid>();
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
         
        public DateTime CreationDate { get; set; }

        public Guid PublicId { get; set; }

        public DateTime LastEntryDate { get; set; }

        public string Title { get;  set; }

        public Guid? CreatedBy { get; set; }

        public string CreatorName { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsPublic { get; set; }

        public List<Guid> SharedPersons { get; set; }

        public string Owner { get; set; }
    }
}