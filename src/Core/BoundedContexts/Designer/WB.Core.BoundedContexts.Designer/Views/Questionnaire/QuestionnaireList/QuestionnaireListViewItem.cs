using System;
using System.Collections.Generic;
using System.Diagnostics;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    [DebuggerDisplay("{Title} | public: {IsPublic}, shared with {SharedPersons.Count} persons")]
    public class QuestionnaireListViewItem
    {
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

        public virtual ISet<Guid> SharedPersons { get; set; } = new HashSet<Guid>();

        public virtual string Owner { get; set; }
    }
}