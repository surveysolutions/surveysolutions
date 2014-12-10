using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    class when_interview_Restored_event_recived_and_interview_summary_is_missing : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportRepositoryWriter>();

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(dataExportRepositoryWriter: dataExportService.Object);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewRestored(Guid.NewGuid()),
                interviewId));

        It should_never_call_method_AddInterviewAction_of_dataExport = () =>
         dataExportService.Verify(x => x.AddInterviewAction(Moq.It.IsAny<InterviewExportedAction>(), interviewId, Moq.It.IsAny<Guid>(), Moq.It.IsAny<DateTime>()), Times.Never);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
    }
}
