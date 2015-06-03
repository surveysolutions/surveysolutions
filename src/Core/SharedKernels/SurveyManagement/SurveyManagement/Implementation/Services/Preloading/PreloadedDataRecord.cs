using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading
{
    public class PreloadedDataRecord
    {
        public PreloadedDataDto PreloadedDataDto {set; get; }
        public Guid? SupervisorId { set; get; }
    }
}
