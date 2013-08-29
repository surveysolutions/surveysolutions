using System;
using System.Collections.Generic;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// The interviewer view.
    /// </summary>
    public class InterviewerView
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerView"/> class.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="groupViews">
        /// The group views.
        /// </param>
        public InterviewerView(string userName, Guid userId, List<InterviewerGroupView> groupViews)
        {
            this.UserId = userId;
            this.UserName = userName;
            this.Items = groupViews;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the items.
        /// </summary>
        public List<InterviewerGroupView> Items { get; private set; }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName { get; private set; }

        #endregion
    }
}