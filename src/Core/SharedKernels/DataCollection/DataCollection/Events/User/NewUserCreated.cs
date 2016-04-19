using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

// ReSharper disable once CheckNamespace
namespace Main.Core.Events.User
{
    [Obsolete("This event must be deleted after all clients who STARTED data collection with version 5.6 or earlier will fade away. " +
              "Pay attentions on the word 'STARTED'." +
              "If the client started with 5.5 and had been updated later to 5.8 he still might have the event which need to be handled")]
    public class NewUserCreated : IEvent
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