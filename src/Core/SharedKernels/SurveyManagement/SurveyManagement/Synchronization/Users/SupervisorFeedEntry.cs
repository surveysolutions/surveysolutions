using System;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.SharedKernels.SurveyManagement.Synchronization.Users
{
    public class SupervisorFeedEntry : IReadSideRepositoryEntity
    {
        public SupervisorFeedEntry(string supervisorId, string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                throw new ArgumentNullException("entryId");
            }
            if (string.IsNullOrEmpty(supervisorId))
            {
                throw new ArgumentNullException("supervisorId");
            }

            this.SupervisorId = supervisorId;
            this.EntryId = entryId;
        }

        public string SupervisorId { get; set; }

        public string EntryId { get; private set; }
    }
}