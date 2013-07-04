namespace Main.Core.Domain
{
    using System;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.User;

    using Ncqrs.Domain;

    /// <summary>
    /// Aggregate root for User.
    /// </summary>
    public class UserAR : AggregateRootMappedByConvention
    {
        #region Fields

        /// <summary>
        /// Email of the user.
        /// </summary>
        private string email;

        /// <summary>
        /// The is locked.
        /// </summary>
        private bool isUserLocked;

        /// <summary>
        /// User Password Hash.
        /// </summary>
        private string password;

        /// <summary>
        /// The roles.
        /// </summary>
        private UserRoles[] roles;

        /// <summary>
        /// The supervisor.
        /// </summary>
        private UserLight supervisor;

        /// <summary>
        /// Name of the user.
        /// </summary>
        private string userName;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAR"/> class.
        /// </summary>
        public UserAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAR"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="roles">
        /// The roles.
        /// </param>
        /// <param name="isLocked">
        /// The is locked.
        /// </param>
        /// <param name="supervisor">
        /// The supervisor.
        /// </param>
        public UserAR(
            Guid publicKey, 
            string userName, 
            string password, 
            string email, 
            UserRoles[] roles, 
            bool isLocked, 
            UserLight supervisor)
            : base(publicKey)
        {
            //// Check for uniqueness of person name and email!
            this.ApplyEvent(
                new NewUserCreated
                    {
                        Name = userName, 
                        Password = password, 
                        Email = email, 
                        IsLocked = isLocked, 
                        Roles = roles, 
                        Supervisor = supervisor, 
                        PublicKey = publicKey
                    });
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Makes changes to user.
        /// </summary>
        /// <param name="email">
        /// User Email. 
        /// </param>
        /// <param name="isLocked">
        /// Is user Locked. 
        /// </param>
        /// <param name="roles">
        /// Roles for User. 
        /// </param>
        public void ChangeUser(string email, bool isLocked, UserRoles[] roles)
        {
            this.ApplyEvent(new UserChanged { Email = email, Roles = roles });

            if (isLocked)
            {
                this.ApplyEvent(new UserLocked());
            }
            else
            {
                this.ApplyEvent(new UserUnlocked());
            }
        }

        public void Lock()
        {
            this.ApplyEvent(new UserLocked());
        }

        public void Unlock()
        {
            this.ApplyEvent(new UserUnlocked());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event handler for the NewUserCreated event. This method
        /// is automaticly wired as event handler based on convension.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewUserCreated(NewUserCreated e)
        {
            this.userName = e.Name;
            this.email = e.Email;
            this.password = e.Password;
            this.isUserLocked = e.IsLocked;
            this.roles = e.Roles;
            this.supervisor = e.Supervisor;
        }

        protected void OnUserLocked(UserLocked @event)
        {
            this.isUserLocked = true;
        }

        protected void OnUserUnlocked(UserUnlocked @event)
        {
            this.isUserLocked = false;
        }

        /// <summary>
        /// The on user change.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnUserChange(UserChanged e)
        {
            this.email = e.Email;
            this.roles = e.Roles;
        }

        #endregion
    }
}