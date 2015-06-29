using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    /// <summary>
    /// The new user created.
    /// </summary>
    [EventName("RavenQuestionnaire.Core:Events:UserCreated")]
    public class NewUserCreated
    {
        public string Email { get; set; }

        //means locked by HQ. For Backward compatibility
        public bool IsLocked { get; set; }
        
        public bool IsLockedBySupervisor { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the roles.
        /// </summary>
        public UserRoles[] Roles { get; set; }

        /// <summary>
        /// Gets or sets the supervisor.
        /// </summary>
        public UserLight Supervisor { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }
    }
}