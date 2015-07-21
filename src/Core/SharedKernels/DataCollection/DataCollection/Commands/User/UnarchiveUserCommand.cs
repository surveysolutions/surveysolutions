using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class UnarchiveUserCommand : CommandBase
    {
        public UnarchiveUserCommand(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; set; } 
    }
}