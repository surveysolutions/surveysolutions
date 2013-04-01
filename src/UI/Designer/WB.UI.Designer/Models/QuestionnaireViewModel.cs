// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireViewModel.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The questionnaire view model.
    /// </summary>
    [DisplayName("Create Questionnaire")]
    public class QuestionnaireViewModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9_]{3,50}$", ErrorMessage = "Questionnaire name needs to be between 3 and 50 characters and contains only letters, digits and symbol \"_\".")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        #endregion
    }
}