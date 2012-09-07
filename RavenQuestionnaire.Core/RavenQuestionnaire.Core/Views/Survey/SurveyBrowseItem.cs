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
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.SubEntities;

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
            this.Grid = new Dictionary<string, int>();
            List<string> statuses = SurveyStatus.GetAllStatuses().Select(s => s.Name).ToList();
            statuses.Insert(0, "Total");
            statuses.Insert(1, "Unassigned");
            foreach (string statuse in statuses)
            {
                this.Grid.Add(statuse, 0);
            }
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
        /// <param name="unAssigment">
        /// The un assigment.
        /// </param>
        /// <param name="statistic">
        /// The statistic.
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
        public SurveyBrowseItem(
            Guid id, 
            string title, 
            int unAssigment, 
            Dictionary<Guid, SurveyItem> statistic, 
            int total, 
            int initial, 
            int error, 
            int completed, int approve)
            : this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unAssigment;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Complete = completed;
            this.Grid["Total"] = this.Total;
            this.Grid["Unassigned"] = unAssigment;
            this.Statistic = statistic;
            this.Grid["Initial"] = initial;
            this.Grid["Approve"] = approve;
            this.Grid["Complete"] = completed;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the grid.
        /// </summary>
        public Dictionary<string, int> Grid { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }

        /// <summary>
        /// Gets or sets the statistic.
        /// </summary>
        public Dictionary<Guid, SurveyItem> Statistic { get; set; }

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

        /// <summary>
        /// Gets or sets Approve.
        /// </summary>
        public int Approve { get; set; }

        #endregion
    }

    /// <summary>
    /// The survey item.
    /// </summary>
    public class SurveyItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyItem"/> class.
        /// </summary>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="endDate">
        /// The end date.
        /// </param>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public SurveyItem(
            DateTime startDate, 
            DateTime endDate, 
            Guid completeQuestionnaireId, 
            SurveyStatus status, 
            UserLight responsible)
        {
            this.Status = status;
            this.EndDate = endDate;
            this.StartDate = startDate;
            this.Responsible = responsible;
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete questionnaire id.
        /// </summary>
        public Guid CompleteQuestionnaireId { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the responsible.
        /// </summary>
        public UserLight Responsible { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        #endregion
    }
}