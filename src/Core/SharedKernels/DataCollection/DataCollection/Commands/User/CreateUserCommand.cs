using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    [Serializable]
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
            bool isLockedBySupervisor, 
            bool isLockedByHQ,
            UserLight supervsor)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.Password = password;
            this.UserName = userName;
            this.Email = email;
            this.Roles = roles;
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;
            this.Supervisor = supervsor;
        }

        public string Email { get; set; }

        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }
        
        public string Password { get; set; }

        public Guid PublicKey { get; set; }

        public UserRoles[] Roles { get; set; }

        public UserLight Supervisor { get; set; }

        public string UserName { get; set; }
    }
}