namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentIdentifyingQuestionRow
    {
        public AssignmentIdentifyingQuestionRow(string title, string answer)
        {
            this.Title = title;
            this.Answer = answer;
        }

        public string Title { get; set; }

        public string Answer { get; set; }
    }
}