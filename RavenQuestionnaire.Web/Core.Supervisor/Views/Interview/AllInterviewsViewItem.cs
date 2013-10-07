using System;
using System.Collections.Generic;
using Core.Supervisor.Views.Interviews;
using Main.Core.Entities.SubEntities;

namespace Core.Supervisor.Views.Interview
{
    public class AllInterviewsViewItem : BaseInterviewGridItem
    {
        public bool CanDelete { get; set; }
    }
}