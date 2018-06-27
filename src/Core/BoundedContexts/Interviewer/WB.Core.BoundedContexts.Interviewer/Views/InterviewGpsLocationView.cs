using System;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    [Obsolete("Remove when all clients are upgrated to 5.13")]
    public class InterviewGpsLocationView
    {
        public int OID { get; set; }
        public Guid? PrefilledQuestionId { get; set; }
        public InterviewGpsCoordinatesView Coordinates { get; set; }
    }
}
