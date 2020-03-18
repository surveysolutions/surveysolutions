﻿using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;
using WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Jobs;

namespace WB.Core.BoundedContexts.Headquarters.Users.UserPreloading.Tasks
{
    public class AssignmentsVerificationTask : BaseTask
    {
        public AssignmentsVerificationTask(IScheduler scheduler) : base(scheduler, "Verification assignments", typeof(AssignmentsVerificationJob)) { }
    }
}
