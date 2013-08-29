using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.View.User
{
    /// <summary>
    /// The user browse item.
    /// </summary>
    public class UserBrowseItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBrowseItem"/> class.
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
        /// <param name="supervisor">
        /// The supervisor.
        /// </param>
        /// <param name="location">
        /// The location.
        /// </param>
        public UserBrowseItem(Guid id, string name, string email, DateTime creationDate, bool isLocked, UserLight supervisor, string location)
        {
            this.Id = id;
            this.Email = email;
            this.UserName = name;
            this.IsLocked = isLocked;
            this.LocationName = location;
            this.CreationDate = creationDate;
            if (supervisor != null) 
                this.SupervisorName = supervisor.Name;
        }

        #endregion

        #region Public Properties

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
        /// Gets a value indicating whether is locked.
        /// </summary>
        public bool IsLocked { get; private set; }

        /// <summary>
        /// Gets or sets the location name.
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the supervisor name.
        /// </summary>
        public string SupervisorName { get; set; }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string UserName { get; private set; }

        #endregion
    }
}