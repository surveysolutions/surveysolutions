using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public class AssignmentApiView
    {
        public string Id { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public int? Capacity { get; set; }
        public int Quantity { get; set; }

        public List<IdentifyingAnswer> IdentifyingData { get; set; } = new List<IdentifyingAnswer>();

        public class IdentifyingAnswer
        {
            public Guid QuestionId { get; set; }
            public string Answer { get; set; }
        }
    }
}