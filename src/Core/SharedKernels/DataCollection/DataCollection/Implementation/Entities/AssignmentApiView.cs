using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class AssignmentApiView
    {
        public int Id { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public int? Quantity { get; set; }
        public int InterviewsCount { get; set; }
        public List<InterviewSerializedAnswer> Answers { get; set; } = new List<InterviewSerializedAnswer>();
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }

        public class InterviewSerializedAnswer
        {
            public Identity Identity { get; set; }
            public string SerializedAnswer { get; set; }
            public string AnswerAsString { get; set; }

            
        }
    }
}