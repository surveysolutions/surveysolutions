using System;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class SyncStatisticsRequest : ICommunicationMessage
    {
        public SyncStatisticsApiView Statistics { get; set; }

        public Guid UserId { get; set; }

        public SyncStatisticsRequest(SyncStatisticsApiView statistics, Guid userId)
        {
            Statistics = statistics;
            UserId = userId;
        }
    }
}
