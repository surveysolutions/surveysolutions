namespace WB.Core.SharedKernels.Enumerator.Entities.Interview
{
    public class InterviewRoster : InterviewEnablementState
    {
        public decimal[] ParentRosterVector { get; set; }
        public decimal RowCode { get; set; }
        public string Title { get; set; }
    }
}