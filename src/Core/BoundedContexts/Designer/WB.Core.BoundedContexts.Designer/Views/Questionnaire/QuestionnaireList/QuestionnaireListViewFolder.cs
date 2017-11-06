using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListViewFolder : IQuestionnaireListItem
    {
        public virtual Guid PublicId { get; set; }

        public virtual string Title { get; set; }

        public virtual Guid? Parent { get; set; }

        public virtual DateTime CreateDate { get; set; }

        public virtual Guid CreatedBy { get; set; }

        public virtual int Depth { get; set; }

        public virtual string Path { get; set; }

        public virtual ISet<QuestionnaireListViewFolder> SubFolders { get; set; }

        public virtual ISet<QuestionnaireListViewFolder> Questionnaires { get; set; }
    }
}