﻿using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public class IncomingDataInfo
    {
        public DataFlowDirection FlowDirection { get; set; }
        public string Endpoint { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ClientComment { get; set; }

        public long BytesTransfered { get; set; }
        public long TotalBytes { get; set; }
        public double BytesPerSecond { get; set; }
        public bool IsCompleted { get; set; }

        public override string ToString()
        {
            return $"{FlowDirection.ToString()} # {Name} message {Type}. {NumericTextFormatter.FormatBytesHumanized(BytesTransfered)} of {NumericTextFormatter.FormatBytesHumanized(TotalBytes)} ";
        }
    }
}
