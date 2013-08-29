namespace Core.Supervisor.Views.Interviewer
{
    using System;

    /// <summary>
    /// The interviewers input model.
    /// </summary>
    public class InterviewersInputModel : ListViewModelBase
    {

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewersInputModel"/> class.
        /// </summary>
        public InterviewersInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the supervisor id.
        /// </summary>
        public Guid ViewerId { get; set; }

        #endregion
    }
}