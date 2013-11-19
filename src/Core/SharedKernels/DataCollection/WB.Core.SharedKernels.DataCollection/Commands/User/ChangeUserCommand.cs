using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(UserAR), "ChangeUser")]
    public class ChangeUserCommand : CommandBase
    {
        public ChangeUserCommand()
        {
        }

        public ChangeUserCommand(Guid publicKey, string email, UserRoles[] roles, bool isLocked, string passwordHash)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.Email = email;
            this.Roles = roles;
            this.IsLocked = isLocked;
            this.PasswordHash = passwordHash;
        }

        public string Email { get; set; }

        public bool IsLocked { get; set; }

        public string PasswordHash { get; set; }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public UserRoles[] Roles { get; set; }
    }
}