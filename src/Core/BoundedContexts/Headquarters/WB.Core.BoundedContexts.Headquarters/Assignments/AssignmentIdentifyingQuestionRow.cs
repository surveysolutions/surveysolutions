using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentIdentifyingQuestionRow
    {
        public AssignmentIdentifyingQuestionRow(string title, string answer, Identity identity, string variable)
        {
            this.Title = title;
            this.Answer = answer;
            this.Identity = identity;
            this.Variable = variable;
        }

        public string Title { get; set; }

        public string Answer { get; set; }

        public Identity Identity { get; }

        public string Variable { get; }
    }
}
