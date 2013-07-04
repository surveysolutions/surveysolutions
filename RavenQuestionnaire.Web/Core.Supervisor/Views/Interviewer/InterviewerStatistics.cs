using System;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InterviewerStatistics
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets CQ Id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets Status.
        /// </summary>
        public SurveyStatus Status { get; set; }

        /// <summary>
        /// Gets or sets TemplateId.
        /// </summary>
        public Guid TemplateId { get; set; }

        /// <summary>
        /// Gets or sets Title.
        /// </summary>
        public string Title { get; set; }

        #endregion

      /*  /// <param name="status">
        /// Current CQ status
        /// </param>
        public void ChangeStatus(string title, SurveyStatus status)
        {
            InterviewerStatistics item = this.StatusesByCQ.FirstOrDefault(s => s.Id == publicKey);
            if (item != null)
            {
                this.Status = status;
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
        }*/
    }
}