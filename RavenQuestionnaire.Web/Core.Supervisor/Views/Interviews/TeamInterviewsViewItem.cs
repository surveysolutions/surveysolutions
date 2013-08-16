using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamInterviewsViewItem
    {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }
        public Guid InterviewId { get; set; }
        public Guid ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }
        public UserRoles ResponsibleRole { get; set; }

        public string Status { get; set; }
        public string LastEntryDate { get; set; }
        public bool CanBeReassigned { get; set; }
    }
}
