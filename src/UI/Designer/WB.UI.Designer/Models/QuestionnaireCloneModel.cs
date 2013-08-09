namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The questionnaire clone model.
    /// </summary>
    [DisplayName("Clone Questionnaire")]
    public class QuestionnaireCloneModel : QuestionnaireViewModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        #endregion
    }
}