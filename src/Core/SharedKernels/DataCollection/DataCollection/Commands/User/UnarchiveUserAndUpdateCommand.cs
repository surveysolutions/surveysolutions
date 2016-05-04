using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class UnarchiveUserAndUpdateCommand : CommandBase
    {
        public UnarchiveUserAndUpdateCommand(Guid userId, string passwordHash, string email, string personName, string phoneNumber)
            : base(userId)
        {
            this.UserId = userId;
            this.PasswordHash = passwordHash;
            this.Email = email;
            this.PersonName = personName;
            this.PhoneNumber = phoneNumber;
        }

        public Guid UserId { get; set; } 

        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public string PersonName { get; set; }

        public string PhoneNumber { get; set; }
    }
}