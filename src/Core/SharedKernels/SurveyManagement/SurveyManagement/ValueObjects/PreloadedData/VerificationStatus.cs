using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData
{
    public class VerificationStatus
    {
        public VerificationStatus()
        {
            WasSupervisorProvided = false;
        }

        public IEnumerable<PreloadedDataVerificationError> Errors { set; get; }

        public bool WasSupervisorProvided { set; get; }
    }
}
