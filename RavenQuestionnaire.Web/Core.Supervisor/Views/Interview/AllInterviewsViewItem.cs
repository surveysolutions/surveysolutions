using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interview
{
    public class AllInterviewsViewItem
    {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public Guid InterviewId { get; set; }
        
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public UserRoles ResponsibleRole { get; set; }

        public string Status { get; set; }
        public string LastEntryDate { get; set; }
        public bool CanDelete { get; set; }
    }
}