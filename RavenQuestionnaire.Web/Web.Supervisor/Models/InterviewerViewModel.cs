// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerViewModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System;

    /// <summary>
    /// The interviewer view model.
    /// </summary>
    public class InterviewerViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the supervisor id.
        /// </summary>
        public Guid SupervisorId { get; set; }

        #endregion
    }
}