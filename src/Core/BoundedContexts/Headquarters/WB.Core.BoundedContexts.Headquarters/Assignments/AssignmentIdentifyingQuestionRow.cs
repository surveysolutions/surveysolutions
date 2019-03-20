using System;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentIdentifyingQuestionRow
    {
        public AssignmentIdentifyingQuestionRow(string title, string answer, Identity identity)
        {
            this.Title = title;
            this.Answer = answer;
            this.Identity = identity;
        }

        public string Title { get; set; }

        public string Answer { get; set; }
        public Identity Identity { get; }
    }
}
