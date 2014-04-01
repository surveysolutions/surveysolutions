using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.Interview.Views
{
    public class AllInterviewsViewItem 
    {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public Guid InterviewId { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public UserRoles ResponsibleRole { get; set; }
        public bool HasErrors { get; set; }
        public string Status { get; set; }
        public string LastEntryDate { get; set; }
        public bool CanDelete { get; set; }
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { set; get; }
    }
}