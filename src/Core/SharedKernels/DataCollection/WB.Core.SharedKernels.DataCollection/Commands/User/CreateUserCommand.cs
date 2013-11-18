using System;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(UserAR))]
    public class CreateUserCommand : CommandBase
    {
        public CreateUserCommand()
        {
        }

        public CreateUserCommand(
            Guid publicKey, 
            string userName, 
            string password, 
            string email, 
            UserRoles[] roles, 
            bool isLocked, 
            UserLight supervsor)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.Password = password;
            this.UserName = userName;
            this.Email = email;
            this.Roles = roles;
            this.IsLocked = isLocked;
            this.Supervisor = supervsor;
        }

        public string Email { get; set; }

        public bool IsLocked { get; set; }

        public string Password { get; set; }

        public Guid PublicKey { get; set; }

        public UserRoles[] Roles { get; set; }

        public UserLight Supervisor { get; set; }

        public string UserName { get; set; }
    }
}