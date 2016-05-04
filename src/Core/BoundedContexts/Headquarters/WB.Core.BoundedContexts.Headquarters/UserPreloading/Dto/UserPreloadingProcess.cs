using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class UserPreloadingProcess
    {
        public UserPreloadingProcess()
        {
            UserPrelodingData = new List<UserPreloadingDataRecord>();
            VerificationErrors = new HashSet<UserPreloadingVerificationError>();
        }

        public virtual string UserPreloadingProcessId { get; set; }
        public virtual string FileName { get; set; }
        public virtual UserPrelodingState State { get; set; }
        public virtual long FileSize { get; set; }
        public virtual DateTime UploadDate { get; set; }
        public virtual DateTime? ValidationStartDate { get; set; }
        public virtual int VerificationProgressInPercents { get; set; }
        public virtual DateTime? CreationStartDate { get; set; }
        public virtual DateTime LastUpdateDate { get; set; }

        public virtual long RecordsCount { get; set; }
        public virtual long CreatedUsersCount { get; set; }

        public virtual string ErrorMessage { get; set; }
        
        [IgnoreDataMember]
        public virtual IList<UserPreloadingDataRecord> UserPrelodingData { get; protected set; }

        public virtual ISet<UserPreloadingVerificationError> VerificationErrors { get; protected set; }
    }
}