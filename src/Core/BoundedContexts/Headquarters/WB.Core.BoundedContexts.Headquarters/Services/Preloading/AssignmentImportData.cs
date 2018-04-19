using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public class InterviewImportData
    {
        public Guid? SupervisorId { get; set; }
        public Guid? InterviewerId { get; set; }
        public PreloadedDataDto PreloadedData { get; set; }
    }

    public class AssignmentImportData : InterviewImportData
    {
        public int? Quantity { get; set; }
    }
}
