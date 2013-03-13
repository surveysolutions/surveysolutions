// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireListViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire list view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The questionnaire list view model.
    /// </summary>
    public class QuestionnaireListViewModel : QuestionnaireViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the creation date.
        /// </summary>
        [Display(Name = "Creation Date", Order = 2)]
        public DateTime CreationDate { get; set; }

        /// <summary>
        ///     Gets or sets the last entry date.
        /// </summary>
        [Display(Name = "Last Entry Date", Order = 3)]
        public DateTime LastEntryDate { get; set; }

        #endregion
    }
}