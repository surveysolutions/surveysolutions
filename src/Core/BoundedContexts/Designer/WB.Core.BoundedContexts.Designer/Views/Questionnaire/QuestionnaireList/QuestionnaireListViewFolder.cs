using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListViewFolder
    {
        public virtual Guid Id { get; set; }

        public virtual string Title { get; set; }

        public virtual Guid? Parent { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual Guid CreatedBy { get; set; }
    }
}