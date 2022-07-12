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
        private ICollection<SharedPerson>? _sharedPersons;

        public QuestionnaireListViewItem()
        {
            _sharedPersons = new List<SharedPerson>();
        }

        public virtual DateTime CreationDate { get; set; }

        public virtual string QuestionnaireId { get; set; } = string.Empty;

        public virtual Guid PublicId
        {
            get
            {
                var id = this.QuestionnaireId.ParseGuid();
                if (id == null)
                    throw new InvalidOperationException("Invalid Questionnaire id.");
                return id.Value;
            }
            set => this.QuestionnaireId = value.FormatGuid();
        }

        public virtual DateTime LastEntryDate { get; set; }

        public virtual string Title { get; set; } = String.Empty;

        public virtual Guid? OwnerId { get; set; }
        public virtual Guid? CreatedBy { get; set; }

        public virtual string? CreatorName { get; set; } 

        public virtual bool IsDeleted { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual ICollection<SharedPerson> SharedPersons
        {
            get => _sharedPersons ?? throw new InvalidOperationException("Trying to use shared persons collection without including it in query");
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _sharedPersons = value;
            }
        }

        public virtual string? Owner { get; set; }

        public virtual Guid? FolderId { get; set; }

        public virtual QuestionnaireListViewFolder? Folder { get; set; }
    }
}
