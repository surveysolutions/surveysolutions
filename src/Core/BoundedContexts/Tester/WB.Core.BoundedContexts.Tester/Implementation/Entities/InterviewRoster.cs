namespace WB.Core.BoundedContexts.Tester.Implementation.Entities
{
    public class InterviewRoster : InterviewGroup
    {
        public decimal[] ParentRosterVector { get; set; }
        public decimal RowCode { get; set; }
        public string Title { get; set; }
    }
}