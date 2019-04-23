using System;
using System.Collections.Generic;
using System.Diagnostics;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    [DebuggerDisplay("{Title} | public: {IsPublic}, shared with {SharedPersons.Count} persons")]
    public class QuestionnaireListViewItem : IQuestionnaireListItem
    {
        public virtual DateTime CreationDate { get; set; }

        public virtual string QuestionnaireId { get; set; }

        public virtual Guid PublicId
        {
            get => this.QuestionnaireId.ParseGuid().Value;
            set => this.QuestionnaireId = value.FormatGuid();
        }

        public virtual DateTime LastEntryDate { get; set; }

        public virtual string Title { get; set; }

        public virtual Guid? CreatedBy { get; set; }

        public virtual string CreatorName { get; set; }

        public virtual bool IsDeleted { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual ICollection<SharedPerson> SharedPersons { get; set; } = new HashSet<SharedPerson>();

        public virtual string Owner { get; set; }

        public virtual Guid? FolderId { get; set; }

        public virtual QuestionnaireListViewFolder Folder { get; set; }
    }
}
