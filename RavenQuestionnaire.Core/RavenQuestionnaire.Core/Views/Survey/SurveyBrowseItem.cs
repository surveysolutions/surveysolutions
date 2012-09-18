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
        /// <param name="approve">
        /// The approve.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public SurveyBrowseItem(
            Guid id, 
            string title, 
            int unAssigment, 
            Dictionary<Guid, SurveyItem> statistic, 
            int total, 
            int initial, 
            int error, 
            int completed, 
            int approve, 
            int redo,
            Dictionary<Guid, string> headers)
            : this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unAssigment;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Completed = completed;
            foreach (var header in headers)
            {
                if (header.Value == SurveyStatus.Initial.Name)
                {
                    this.Grid.Add(header.Value, initial);
                }

                if (header.Value == SurveyStatus.Approve.Name)
                {
                    this.Grid.Add(header.Value, approve);
                }

                if (header.Value == SurveyStatus.Complete.Name)
                {
                    this.Grid.Add(header.Value, completed);
                }

                if (header.Value == SurveyStatus.Error.Name)
                {
                    this.Grid.Add(header.Value, error);
                }

                if (header.Value == "Total")
                {
                    this.Grid.Add(header.Value, this.Total);
                }

                if (header.Value == "Unassigned")
                {
                    this.Grid.Add(header.Value, unAssigment);
                }

                if (header.Value == "Redo")
                {
                    this.Grid.Add(header.Value, redo);
                }
            }

            this.Statistic = statistic;
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
        /// Gets or sets Redo.
        /// </summary>
        public int Redo { get; set; }

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

        #endregion
    }
}