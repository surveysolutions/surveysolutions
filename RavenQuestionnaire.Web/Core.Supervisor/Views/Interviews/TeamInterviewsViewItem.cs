using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Interview;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interviews
{
    public class TeamInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanBeReassigned { get; set; }
    }
}
