using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        public static StatusChangeHistoryDenormalizerFunctional CreateDenormalizer(IReadSideRepositoryWriter<InterviewStatuses> interviewStatuses=null)
        {
            return new StatusChangeHistoryDenormalizerFunctional(interviewStatuses?? Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>(), Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == new InterviewSummary()));
        }
    }
}
