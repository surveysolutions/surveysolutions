namespace WB.UI.Designer.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The delete questionnaire model.
    /// </summary>
    [DisplayName("Questionnaire")]
    public class DeleteQuestionnaireModel
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [Display(Name = "Title")]
        public string Title { get; set; }
    }
}