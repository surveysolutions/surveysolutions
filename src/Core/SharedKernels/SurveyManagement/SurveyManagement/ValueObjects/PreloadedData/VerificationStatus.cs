using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData
{
    public class VerificationStatus
    {
        public VerificationStatus()
        {
            WasResponsibleProvided = false;
        }

        public IEnumerable<PreloadedDataVerificationError> Errors { set; get; }

        public bool WasResponsibleProvided { set; get; }
    }
}
