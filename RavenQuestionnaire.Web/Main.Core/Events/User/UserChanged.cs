namespace Main.Core.Events.User
{
    using System;
    using System.Linq;

    using Main.Core.Entities.SubEntities;

    using Ncqrs.Eventing.Storage;

    /// <summary>
    /// The user changed.
    /// </summary>
    [Serializable]
    [EventName("RavenQuestionnaire.Core:Events:UserChanged")]
    public class UserChanged : UserBaseEvent
    {
        /* [AggregateRootId]
        public Guid PublicKey { get; set; }*/
        #region Public Properties

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        //// Is not propogated now
        public UserRoles[] Roles { get; set; }

        #endregion

        protected override bool DoCheckIsAssignedRole(UserRoles role)
        {
            return Roles != null && Roles.Where(r => r == role).Count() > 0;
        }
    }
}