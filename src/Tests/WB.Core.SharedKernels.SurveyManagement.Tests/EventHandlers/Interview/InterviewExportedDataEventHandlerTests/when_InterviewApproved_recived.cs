using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_InterviewApproved_recived : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService=new Mock<IDataExportService>();
            interviewActionLogWriter = new Mock<IReadSideRepositoryWriter<InterviewActionLog>>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());
            interviewActionLog = new InterviewActionLog(interviewId, new List<InterviewActionExportView>());

            interviewActionLogWriter.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(interviewActionLog);
            
            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name" }, interviewActionLogWriter.Object);
        };

        Because of = () =>
           interviewExportedDataDenormalizer.Handle(CreateInterviewApprovedByHQPublishableEvent(interviewId));

        It should_interviewActionLog_be_removed = () =>
              interviewActionLogWriter.Verify(x => x.Remove(interviewId.FormatGuid()), Times.Once);

        It should_interviewActionLog_be_added_to_data_export = () =>
           dataExportService.Verify(x => x.AddInterviewActions(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>(), interviewActionLog.Actions), Times.Once);

        It should_InterviewExportedAction_be_present_in_list_of_actions = () =>
            interviewActionLog.Actions[0].Action.ShouldEqual(InterviewExportedAction.ApproveByHeadquarter);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IReadSideRepositoryWriter<InterviewActionLog>> interviewActionLogWriter;
        private static Mock<IDataExportService> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewActionLog interviewActionLog;
    }
}
