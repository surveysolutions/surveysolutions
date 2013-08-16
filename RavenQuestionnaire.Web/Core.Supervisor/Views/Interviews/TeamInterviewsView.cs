using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamInterviewsView 
    {
        public int TotalCount { get; set; }
        public IEnumerable<TeamInterviewsViewItem> Items { get; set; }
    }
}
