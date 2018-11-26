using System;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.QuartzIntegration;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs
{
    public class DeleteQuestionnaireJobScheduler : BaseTask
    {
        public DeleteQuestionnaireJobScheduler(IScheduler scheduler) : base(scheduler, "Delete questionnaire", typeof(DeleteQuestionnaireJob)) { }
    }
}
