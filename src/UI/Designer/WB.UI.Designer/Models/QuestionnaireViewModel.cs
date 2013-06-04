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
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Display(Name = "Is Public")]
        public bool IsPublic { get; set; }

        #endregion
    }
}