using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    /// <summary>
    /// The interviewers item.
    /// </summary>
    public class InterviewersItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewersItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="creationDate">
        /// The creation date.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        public InterviewersItem(Guid id, string name, string email, DateTime creationDate, bool isLocked)
        {
            this.UserId = id;
            this.UserName = name;
            this.Email = email;
            this.CreationDate = creationDate.ToShortDateString();
            this.IsLocked = isLocked;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets a value indicatios user is locked or not.
        /// </summary>
        public bool IsLocked { get; private set; }
        
        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public string CreationDate { get; private set; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// Gets the login.
        /// </summary>
        public string UserName { get; private set; }

        #endregion
    }
}