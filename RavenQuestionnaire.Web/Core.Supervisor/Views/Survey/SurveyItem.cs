using System;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Survey
{
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