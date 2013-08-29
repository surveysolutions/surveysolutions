namespace Web.Supervisor.Models
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     The interviewer view model.
    /// </summary>
    [DisplayName("Interviewer")]
    public class InterviewerViewModel : UserViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        #endregion
    }
}