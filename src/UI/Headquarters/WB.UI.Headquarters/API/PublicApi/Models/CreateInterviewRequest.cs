using System;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class CreateInterviewRequest
    {
        /// <summary>
        /// Assignment Id
        /// </summary>
        public int AssignmentId { get; set; }
    }

    public class CreateInterviewResult
    {
        public int AssignmentId { get; set; }
        public Guid InterviewId { get; set; }
        public string InterviewKey { get; set; }
        public string AssignedTo { get; set; }
        public Guid AssignedToId { get; set; }
    }
}