using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class ChangeUserCommand : CommandBase
    {
        public ChangeUserCommand()
        {
        }

        public ChangeUserCommand(Guid publicKey, string email, UserRoles[] roles, bool? isLockedBySupervisor, bool isLockedByHQ,
            string passwordHash, string personName, string phoneNumber, Guid userId)
            : base(publicKey)
        {
            this.PublicKey = publicKey;
            this.Email = email;
            this.Roles = roles;
            this.IsLockedBySupervisor = isLockedBySupervisor;
            this.IsLockedByHQ = isLockedByHQ;

            this.PasswordHash = passwordHash;

            this.PhoneNumber = phoneNumber;
            this.PersonName = personName;

            this.UserId = userId;
        }

        public string Email { get; set; }

        public bool? IsLockedBySupervisor { get; set; }

        public bool IsLockedByHQ { get; set; }

        public string PasswordHash { get; set; }

        public Guid PublicKey { get; set; }

        public UserRoles[] Roles { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }

        public Guid UserId { get; set; }
    }
}