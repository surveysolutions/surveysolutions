using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization
{
    public class CommentSynchronizationDto
    {
        public string Text { get; set; }

        public DateTime Date { get; set; }

        public Guid UserId { get; set; }

        public UserRoles UserRole { get; set; }
    }
}