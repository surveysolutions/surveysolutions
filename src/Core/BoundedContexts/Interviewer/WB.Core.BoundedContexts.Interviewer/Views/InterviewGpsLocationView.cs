using System;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewGpsLocationView
    {
        public int OID { get; set; }
        public Guid? PrefilledQuestionId { get; set; }
        public InterviewGpsCoordinatesView Coordinates { get; set; }
    }
}