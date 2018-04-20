using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class BaseInterviewGridItem {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public Guid InterviewId { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public UserRoles ResponsibleRole { get; set; }
        public int ErrorsCount { get; set; }
        public string Status { get; set; }
        public DateTime LastEntryDateUtc { get; set; }
        public int? AssignmentId { get; set; }

    }
}
