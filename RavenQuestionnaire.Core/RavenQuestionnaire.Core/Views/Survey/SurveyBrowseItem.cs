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
        /// <param name="unAssigned">
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
        public SurveyBrowseItem(Guid id, string title, int unAssigned, Dictionary<Guid, SurveyItem> statistic, 
                                int total, int initial, int error, int completed, int approve, 
                                Dictionary<Guid, string> headers): this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unAssigned;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Completed = completed;
            this.Statistic = statistic;
            foreach (var header in headers)
            {
                if (header.Value==SurveyStatus.Initial.Name) this.Grid.Add(header.Value, initial);
                if (header.Value == SurveyStatus.Approve.Name) this.Grid.Add(header.Value, approve);
                if (header.Value == SurveyStatus.Complete.Name) this.Grid.Add(header.Value, completed);
                if (header.Value == SurveyStatus.Error.Name) this.Grid.Add(header.Value, error);
                if (header.Value == "Total") this.Grid.Add(header.Value, this.Total);
                if (header.Value == "Unassigned") this.Grid.Add(header.Value, unAssigned);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Completed { get; set; }

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