// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewersItem.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewers item.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.User
{
    using System;

    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The interviewers item.
    /// </summary>
    public class InterviewersItem
    {
        #region Fields

        #endregion

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
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="completed">
        /// The completed.
        /// </param>
        /// <param name="inProcess">
        /// The in process.
        /// </param>
        public InterviewersItem(
            Guid id, 
            string name, 
            string email, 
            DateTime creationDate, 
            bool isLocked, 
            int total, 
            int completed, 
            int inProcess)
        {
            this.Id = id;
            this.Login = name;
            this.Email = email;
            this.CreationDate = creationDate;
            this.IsLocked = isLocked;
            this.Total = total;
            this.Completed = completed;
            this.InProcess = inProcess;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the completed.
        /// </summary>
        public int Completed { get; private set; }

        /// <summary>
        /// Gets the creation date.
        /// </summary>
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the in process.
        /// </summary>
        public int InProcess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// Gets the login.
        /// </summary>
        public string Login { get; private set; }

        /// <summary>
        /// Gets the total.
        /// </summary>
        public int Total { get; private set; }

        #endregion
    }
}