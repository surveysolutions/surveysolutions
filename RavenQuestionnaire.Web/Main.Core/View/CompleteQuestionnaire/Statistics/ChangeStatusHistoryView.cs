// -----------------------------------------------------------------------
// <copyright file="ChangeStatusHistoryView.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.CompleteQuestionnaire.Statistics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ChangeStatusHistoryView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeStatusHistoryView"/> class.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        public ChangeStatusHistoryView(UserLight user, SurveyStatus status)
        {
            this.UserName = user.Name;
            this.StatusName = status.Name;
            this.Comment = status.ChangeComment;
        }

        /// <summary>
        /// Gets or sets Comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets StatusName.
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Gets or sets UserName.
        /// </summary>
        public string UserName { get; set; }
    }
}
