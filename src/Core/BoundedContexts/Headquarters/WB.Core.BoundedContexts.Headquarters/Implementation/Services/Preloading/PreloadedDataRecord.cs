using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading
{
    public class PreloadedDataRecord
    {
        public PreloadedDataDto PreloadedDataDto {set; get; }
        public Guid? SupervisorId { set; get; }
        public Guid? InterviewerId { set; get; }
    }

    public class AssignmentPreloadedDataRecord : PreloadedDataRecord
    {
        public int? Quantity { set; get; }
    }
}
