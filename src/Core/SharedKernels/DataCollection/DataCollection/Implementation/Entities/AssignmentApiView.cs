using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class AssignmentApiView
    {
        public int Id { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public int? Quantity { get; set; }
        public int InterviewsCount { get; set; }

        public List<IdentifyingAnswer> IdentifyingData { get; set; } = new List<IdentifyingAnswer>();

        public class IdentifyingAnswer
        {
            public Identity Identity { get; set; }
            public string Answer { get; set; }
        }
    }
}