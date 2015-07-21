using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.User
{
    public class ArchiveUserCommad : CommandBase
    {
        public ArchiveUserCommad(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; set; }
    }
}