using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    [Obsolete ("v5.3")]
    public class InterviewSyncPackageDto
    {
        public string PackageId { get; set; }

        public string Content { get; set; }

        public string MetaInfo { get; set; }
    }
}