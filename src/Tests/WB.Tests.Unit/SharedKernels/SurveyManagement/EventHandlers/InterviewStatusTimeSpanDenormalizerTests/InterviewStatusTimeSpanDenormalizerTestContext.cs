using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.InterviewStatusTimeSpanDenormalizerTests
{
    [Subject(typeof(InterviewStatusTimeSpanDenormalizer))]
    internal class InterviewStatusTimeSpanDenormalizerTestContext
    {
        protected static InterviewStatusTimeSpanDenormalizer CreateInterviewStatusTimeSpanDenormalizer(IReadSideRepositoryWriter<InterviewStatusTimeSpans> interviewCustomStatusTimestampStorage=null,
            IReadSideRepositoryWriter<InterviewStatuses> statuses = null)
        {
            return new InterviewStatusTimeSpanDenormalizer(interviewCustomStatusTimestampStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewStatusTimeSpans>>(), statuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>());
        }
    }
}
