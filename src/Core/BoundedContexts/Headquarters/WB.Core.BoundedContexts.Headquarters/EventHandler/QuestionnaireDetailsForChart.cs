﻿using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    public class QuestionnaireDetailsForChart : IView
    {
        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }

        public int SupervisorAssignedCount { get; set; }
        public int InterviewerAssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int ApprovedBySupervisorCount { get; set; }
        public int RejectedBySupervisorCount { get; set; }
        public int ApprovedByHeadquartersCount { get; set; }
        public int RejectedByHeadquartersCount { get; set; }
    }
}
