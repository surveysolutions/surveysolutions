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
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The questionnaire list view model.
    /// </summary>
    [DisplayName("questionnaire")]
    public class QuestionnairePublicListViewModel : QuestionnaireListViewModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the title.
        /// </summary>
        [Display(Name = "Created by", Order = 2)]
        public string CreatorName { get; set; }
        
        #endregion
    }
}