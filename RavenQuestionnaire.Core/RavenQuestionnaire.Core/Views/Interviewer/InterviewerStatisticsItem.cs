// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerStatisticsItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewer item view.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Interviewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using RavenQuestionnaire.Core.Entities.SubEntities;

    /// <summary>
    /// Interviewer's statistics item
    /// </summary>
    public class InterviewerStatisticsItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsItem"/> class.
        /// </summary>
        public InterviewerStatisticsItem()
        {
            this.StatusesByCQ = new List<InterviewerStatistics>();
        }

        /// <summary>
        /// Gets or sets interviewer id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets interviewer name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets map of CQ public key and theirs statuses
        /// </summary>
        public List<InterviewerStatistics> StatusesByCQ { get; set; }

        /// <summary>
        /// Add status item to dictionary
        /// </summary>
        /// <param name="publicKey">
        /// Complete questionnaire public key.
        /// </param>
        /// <param name="templateId">
        /// Questionnaire public key
        /// </param>
        /// <param name="status">
        /// Current CQ status
        /// </param>
        public void AddCQ(Guid publicKey, Guid templateId, SurveyStatus status)
        {
            var item = this.StatusesByCQ.FirstOrDefault(s => s.Id == publicKey);
            if (item != null)
            {
                item.Status = status;
            }
            else
            {
                this.StatusesByCQ.Add(new InterviewerStatistics()
                    {
                        Id = publicKey,
                        Status = status,
                        TemplateId = templateId
                    });
            }
        }

        /// <summary>
        /// Remove status item from map
        /// </summary>
        /// <param name="publicKey">
        /// Complete questionnaire public key.
        /// </param>
        public void RemoveCQ(Guid publicKey)
        {
            if (this.StatusesByCQ.Any(s => s.Id == publicKey))
            {
                this.StatusesByCQ.Remove(this.StatusesByCQ.Single(s => s.Id == publicKey));
            }
        }
    }
}
