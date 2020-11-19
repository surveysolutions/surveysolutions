#nullable enable
using System;

namespace WB.UI.Headquarters.Controllers
{
    public class UpdateInterviewCalendarEventRequest
    {
        public Guid? Id { get; set; }
        public string InterviewId { get; set; } = String.Empty;

        public int AssignmentId { get; set; }
        public DateTime? NewDate { get; set; }
        public string Comment { get; set; } = String.Empty;
        public string Timezone { get; set; } = String.Empty;
    } 
}
