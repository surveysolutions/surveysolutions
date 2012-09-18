// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyBrowseItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey browse item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.Survey
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The survey browse item.
    /// </summary>
    public class SurveyBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyBrowseItem"/> class.
        /// </summary>
        public SurveyBrowseItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyBrowseItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        /// <param name="unassigment">
        /// The un assigment.
        /// </param>
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="initial">
        /// The initial.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="completed">
        /// The completed.
        /// </param>
        /// <param name="approve">
        /// The approve.
        /// </param>
        public SurveyBrowseItem(
            Guid id, 
            string title, 
            int unassigment, 
            int total, 
            int initial, 
            int error, 
            int completed, 
            int approve)
            : this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unassigment;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Complete = completed;
            this.Approve = approve;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Approve.
        /// </summary>
        public int Approve { get; set; }

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }
        
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets the unassigned.
        /// </summary>
        public int Unassigned { get; set; }

        #endregion
    }
}