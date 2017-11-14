using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class UserPreloadingProcess
    {
        public UserPreloadingProcess()
        {
            UserPrelodingData = new List<UserPreloadingDataRecord>();
        }

        public virtual string UserPreloadingProcessId { get; set; }
        public virtual string FileName { get; set; }

        public virtual long RecordsCount { get; set; }

        public virtual int SupervisorsCount { get; set; }
        public virtual int InterviewersCount { get; set; }
        
        [IgnoreDataMember]
        public virtual IList<UserPreloadingDataRecord> UserPrelodingData { get; set; }
    }
}