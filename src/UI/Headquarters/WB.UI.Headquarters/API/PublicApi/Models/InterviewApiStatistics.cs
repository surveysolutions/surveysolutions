namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiStatistics
    {
        public int Answered { get; set; }
        public int NotAnswered { get; set; }
        public int Flagged { get; set; }
        public int NotFlagged { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int WithComments { get; set; }
        public int ForInterviewer { get; set; }
        public int ForSupervisor { get; set; }
    }
}