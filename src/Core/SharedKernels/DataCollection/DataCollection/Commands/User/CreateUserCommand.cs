using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
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
            UserLight supervsor,
            string personName, 
            string phoneNumber)
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

            this.PhoneNumber = phoneNumber;
            this.PersonName = personName;
        }

        public string Email { get; set; }

        public bool IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }
        
        public string Password { get; set; }

        public Guid PublicKey { get; set; }

        public UserRoles[] Roles { get; set; }

        public UserLight Supervisor { get; set; }

        public string UserName { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }

    }
}