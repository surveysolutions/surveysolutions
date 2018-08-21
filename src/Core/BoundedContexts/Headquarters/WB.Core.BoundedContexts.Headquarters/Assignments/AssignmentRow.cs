using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentRow
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid ResponsibleId { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public int? Quantity { get; set; }
        public int InterviewsCount { get; set; }
        public int Id { get; set; }
        public string Responsible { get; set; }
        public string ResponsibleRole { get; set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Archived { get; set; }
        public string QuestionnaireTitle { get; set; }

        public List<AssignmentIdentifyingQuestionRow> IdentifyingQuestions { get; set; }
    }
}
