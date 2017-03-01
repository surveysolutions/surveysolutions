using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        public static StatusChangeHistoryDenormalizerFunctional CreateDenormalizer(IReadSideRepositoryWriter<InterviewStatuses> interviewStatuses = null)
        {
            var defultUserDocument = Create.Entity.UserDocument(supervisorId: Guid.NewGuid());
            return
                new StatusChangeHistoryDenormalizerFunctional(
                    interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>(),
                    Mock.Of<IPlainStorageAccessor<UserDocument>>(_ => _.GetById(Moq.It.IsAny<string>()) == defultUserDocument),
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == new InterviewSummary()));
        }
    }
}
