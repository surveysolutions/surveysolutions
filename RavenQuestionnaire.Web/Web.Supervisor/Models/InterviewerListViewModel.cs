namespace Web.Supervisor.Models
{
    using System;

    using Core.Supervisor.Views.Interviewer;

    /// <summary>
    ///     The interviewer list view model.
    /// </summary>
    public class InterviewerListViewModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the interviewers.
        /// </summary>
        public InterviewersView View { get; set; }

        /// <summary>
        ///     Gets or sets the supervisor name.
        /// </summary>
        public string SupervisorName { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        #endregion
    }
}