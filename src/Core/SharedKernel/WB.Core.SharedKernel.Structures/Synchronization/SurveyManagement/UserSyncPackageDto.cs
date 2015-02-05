using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class UserSyncPackageDto : BaseSyncPackageDto
    {
        [Obsolete("Probably used for deserialization")]
        public UserSyncPackageDto()
        {
        }

        public Guid UserId { get; set; }
    }
}