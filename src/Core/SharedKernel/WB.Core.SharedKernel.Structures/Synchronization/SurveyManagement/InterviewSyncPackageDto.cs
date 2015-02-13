using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class InterviewSyncPackageDto : BaseSyncPackageDto
    {
        public Guid InterviewId { get; set; }

        public string VersionedQuestionnaireId { get; set; }

        public Guid UserId { get; set; }

        public string ItemType { get; set; }

        public string MetaInfo { get; set; }

    }
}