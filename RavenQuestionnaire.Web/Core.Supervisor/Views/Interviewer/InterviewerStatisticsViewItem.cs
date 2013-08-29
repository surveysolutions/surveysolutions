using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviewer
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class InterviewerStatisticsViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerStatisticsViewItem"/> class. 
        /// Initializes a new instance of the <see cref="InterviewersItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="title">
        /// Questionnaire template title
        /// </param>
        /// <param name="templateId">
        /// Questionnaire template public key
        /// </param>
        /// <param name="initial">
        /// Initial count
        /// </param>
        /// <param name="error">
        /// Error count
        /// </param>
        /// <param name="complete">
        /// Completded count
        /// </param>
        /// <param name="approve">
        /// Approved count
        /// </param>
        /// <param name="redo">
        /// The redo.
        /// </param>
        public InterviewerStatisticsViewItem(
            Guid id, string name, string title, Guid templateId, int initial, int error, int complete, int approve, int redo)
        {
            this.Id = id;
            this.Login = name;
            this.Title = title;
            this.TemplateId = templateId;
            this.Initial = initial;
            this.Error = error;
            this.Completed = complete;
            this.Approve = approve;
            this.Redo = redo;
            this.Counters = new Dictionary<Guid, int>
                {
                    { SurveyStatus.Initial.PublicId, initial }, 
                    { SurveyStatus.Error.PublicId, error }, 
                    { SurveyStatus.Complete.PublicId, complete }, 
                    { SurveyStatus.Approve.PublicId, approve },
                    { SurveyStatus.Redo.PublicId, redo }
                };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the completed.
        /// </summary>
        public int Approve { get; private set; }

        /// <summary>
        /// Gets the completed.
        /// </summary>
        public int Completed { get; private set; }

        /// <summary>
        /// Gets or sets table headers.
        /// </summary>
        public Dictionary<Guid, int> Counters { get; set; }

        /// <summary>
        /// Gets the completed.
        /// </summary>
        public int Error { get; private set; }

        /// <summary>
        /// Gets Redo.
        /// </summary>
        public int Redo { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the total.
        /// </summary>
        public int Initial { get; private set; }

        /// <summary>
        /// Gets the login.
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// Gets the template id.
        /// </summary>
        public Guid TemplateId { get; private set; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        public string Title { get; private set; }

        #endregion
    }
}