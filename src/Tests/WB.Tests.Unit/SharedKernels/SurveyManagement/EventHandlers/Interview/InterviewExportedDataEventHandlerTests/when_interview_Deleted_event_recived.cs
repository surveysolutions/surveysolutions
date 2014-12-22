using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_interview_Deleted_event_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportRepositoryWriter>();

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(dataExportRepositoryWriter: dataExportService.Object);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new InterviewDeleted(Guid.NewGuid()),
                interviewId));

        It should_call_method_DeleteInterview_of_dataExport = () =>
                dataExportService.Verify(x => x.DeleteInterview(interviewId), Times.Once);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static Mock<IDataExportRepositoryWriter> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
    }
}
