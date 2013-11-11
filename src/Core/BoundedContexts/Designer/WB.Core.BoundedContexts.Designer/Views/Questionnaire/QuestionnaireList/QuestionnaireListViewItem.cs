using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    /// <summary>
    /// The questionnaire browse item.
    /// </summary>
    public class QuestionnaireListViewItem : IView
    {
        #region Constructors and Destructors
        public QuestionnaireListViewItem()
        {
        }

        public QuestionnaireListViewItem(Guid id, string title, DateTime creationDate, DateTime lastEntryDate, Guid? createdBy, bool isPublic)
        {
            this.PublicId = id;
            this.Title = title;
            this.CreationDate = creationDate;
            this.LastEntryDate = lastEntryDate;
            this.CreatedBy = createdBy;
            this.IsPublic = isPublic;
            this.SharedPersons = new List<Guid>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid PublicId { get; private set; }

        /// <summary>
        /// Gets the last entry date.
        /// </summary>
        public DateTime LastEntryDate { get; private set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get;  set; }

        /// <summary>
        /// Gets the creator id.
        /// </summary>
        public Guid? CreatedBy { get; private set; }

        /// <summary>
        /// Gets or sets the creator name.
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is public.
        /// </summary>
        public bool IsPublic { get; set; }

        public List<Guid> SharedPersons { get; private set; }

        public string Owner { get; set; }

        #endregion
    }
}