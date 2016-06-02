﻿using System;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
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