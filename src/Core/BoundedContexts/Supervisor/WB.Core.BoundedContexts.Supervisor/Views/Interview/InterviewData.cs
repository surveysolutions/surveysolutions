using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewData : InterviewBrief
    {
        public InterviewData()
        {
            Levels = new Dictionary<string, InterviewLevel>();
        }

        public UserRoles ResponsibleRole { get; set; }
        public DateTime UpdateDate { get; set; }
        public Dictionary<string, InterviewLevel> Levels { get; set; }
    }
}