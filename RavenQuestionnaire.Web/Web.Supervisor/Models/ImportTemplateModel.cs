// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImportTemplateModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Web.Supervisor.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The import questionnaire model.
    /// </summary>
    [DisplayName("Template")]
    public class ImportTemplateModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password", Order = 3)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the questionnaire id.
        /// </summary>
        [Required]
        [Display(Name = "Questionnaire Id", Order = 1)]
        public Guid QuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        [Required]
        [Display(Name = "User name", Order = 2)]
        public string UserName { get; set; }

        #endregion
    }
}