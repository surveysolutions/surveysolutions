using System;
using WB.Core.SharedKernels.SurveySolutions;

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

            this.SupervisorId = supervisorId;
            this.EntryId = entryId;
        }

        public string SupervisorId { get; set; }

        public string EntryId { get; private set; }
    }
}