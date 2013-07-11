namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The questionnaire list view model.
    /// </summary>
    [DisplayName("questionnaire")]
    public class QuestionnaireListViewModel : ActionItem
    {
        #region Public Properties
        /// <summary>
        ///     Gets a value indicating whether can preview.
        /// </summary>
        public override bool CanPreview
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        [Display(Name = "Title", Order = 1)]
        [Default]
        public string Title { get; set; }

        /// <summary>
        ///     Gets or sets the creation date.
        /// </summary>
        [Display(Name = "Creation Date", Order = 2)]
        public DateTime CreationDate { get; set; }

        /// <summary>
        ///     Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the last entry date.
        /// </summary>
        [Display(Name = "Last Entry Date", Order = 3)]
        public DateTime LastEntryDate { get; set; }

        /// <summary>
        ///     Questionnaire status in system(deleted/not deleted).
        /// </summary>
        [Display(Name = "Deleted?", Order = 4)]
        [OnlyForAdmin]
        public bool IsDeleted { get; set; }

        /// <summary>
        ///    Questionnaire status by availability for other users(public/private).
        /// </summary>
        [Display(Name = "Public?", Order = 5)]
        [OnlyForAdmin]
        public bool IsPublic { get; set; }

        #endregion
    }
}