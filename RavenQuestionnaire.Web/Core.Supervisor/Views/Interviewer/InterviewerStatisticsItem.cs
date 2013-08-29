using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// Interviewer's statistics item
    /// </summary>
    public class InterviewerStatisticsItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsItem"/> class.
        /// </summary>
        public InterviewerStatisticsItem()
        {
            this.StatusesByCQ = new List<InterviewerStatistics>();
        }

        #endregion

        #region Public Properties

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


        #endregion

        #region Public Methods and Operators
      
        /// <summary>
        /// The get table rows.
        /// </summary>
        /// <returns>
        /// List of table rows
        /// </returns>
        public List<InterviewerStatisticsViewItem> GetTableRows()
        {
            var templateGuids = this.StatusesByCQ.Select(s => new { Id = s.TemplateId, s.Title }).Distinct();

            return
                templateGuids.Select(
                    t =>
                    new InterviewerStatisticsViewItem(
                        this.Id, 
                        this.Name, 
                        t.Title, 
                        t.Id, 
                        this.StatusesByCQ.Count(
                            s => (s.TemplateId == t.Id) && (s.Status.PublicId == SurveyStatus.Initial.PublicId)), 
                        this.StatusesByCQ.Count(
                            s => (s.TemplateId == t.Id) && (s.Status.PublicId == SurveyStatus.Error.PublicId)), 
                        this.StatusesByCQ.Count(
                            s => (s.TemplateId == t.Id) && (s.Status.PublicId == SurveyStatus.Complete.PublicId)),
                        this.StatusesByCQ.Count(
                            s => (s.TemplateId == t.Id) && (s.Status.PublicId == SurveyStatus.Approve.PublicId)),
                        this.StatusesByCQ.Count(
                            s => (s.TemplateId == t.Id) && (s.Status.PublicId == SurveyStatus.Redo.PublicId)))).
                    ToList();
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
        /// <param name="status">
        /// Current CQ status
        /// </param>
        public void AddCQ(Guid publicKey, Guid templateId, string title, SurveyStatus status)
        {
            InterviewerStatistics item = this.StatusesByCQ.FirstOrDefault(s => s.Id == publicKey);
            if (item != null)
            {
                item.Status = status;
            }
            else
            {
                this.StatusesByCQ.Add(
                    new InterviewerStatistics
                    {
                        Id = publicKey,
                        Status = status,
                        Title = title,
                        TemplateId = templateId
                    });
            }
        }
        #endregion
    }
}