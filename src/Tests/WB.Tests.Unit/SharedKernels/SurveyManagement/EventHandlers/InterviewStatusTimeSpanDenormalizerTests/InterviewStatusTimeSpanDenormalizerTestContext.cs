using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    [Subject(typeof(InterviewStatusTimeSpanDenormalizer))]
    internal class InterviewStatusTimeSpanDenormalizerTestContext
    {
        protected static InterviewStatusTimeSpanDenormalizer CreateInterviewStatusTimeSpanDenormalizer(IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage=null,
            IReadSideRepositoryWriter<InterviewSummary> statuses = null)
        {
            return new InterviewStatusTimeSpanDenormalizer(interviewCustomStatusTimestampStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewStatusTimeSpans>>(), statuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>());
        }
    }
}
