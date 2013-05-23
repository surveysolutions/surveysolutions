// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewersViewModelInputModel.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Web.Supervisor.Models
{
    using System;

    using Core.Supervisor.Views.Interviewer;

    /// <summary>
    /// The interviewers view model input model.
    /// </summary>
    public class InterviewersViewInputModel : InterviewersInputModel
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        #endregion
    }
}