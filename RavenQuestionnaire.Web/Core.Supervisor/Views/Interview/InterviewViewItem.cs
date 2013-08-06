using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interview
{
    public class InterviewViewItem
    {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public Guid InterviewId { get; set; }
        public UserLight Responsible { get; set; }
        public string Status { get; set; }
        public string Title { get; set; }
        public string LastEntryDate { get; set; }
        public bool CanDelete { get; set; }
    }
}