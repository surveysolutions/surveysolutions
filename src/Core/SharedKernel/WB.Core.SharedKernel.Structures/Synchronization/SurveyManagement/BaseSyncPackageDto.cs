using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public abstract class BaseSyncPackageDto
    {
        public string PackageId { get; set; }

        public string Content { get; set; }

        public DateTime Timestamp { get; set; }

        public long SortIndex { get; set; }
    }
}