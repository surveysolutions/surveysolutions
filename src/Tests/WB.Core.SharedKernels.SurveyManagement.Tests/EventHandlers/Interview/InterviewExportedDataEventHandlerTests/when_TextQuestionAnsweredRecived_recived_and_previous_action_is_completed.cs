using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_TextQuestionAnsweredRecived_recived_and_previous_action_is_completed : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportService>();
            interviewActionLogWriter = new Mock<IReadSideRepositoryWriter<InterviewActionLog>>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());

            interviewActionLog = new InterviewActionLog(interviewId, new List<InterviewActionExportView>());
            interviewActionLog.Actions.Add(new InterviewActionExportView(interviewId.FormatGuid(),
                InterviewExportedAction.Completed, "test", DateTime.Now));

            interviewActionLogWriter.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(interviewActionLog);

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles> { UserRoles.Operator } }, interviewActionLogWriter.Object);
        };

        Because of = () =>
            interviewExportedDataDenormalizer.Handle(CreatePublishableEvent(() => new TextQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "answer"),
                interviewId));

        It should_FirstAnswerSet_action_be_absent_in_list_of_actions = () =>
            interviewActionLog.Actions.Select(a => a.Action).ShouldNotContain(InterviewExportedAction.FirstAnswerSet);

        It should_not_store_InterviewActionLog = () =>
           interviewActionLogWriter.Verify(x => x.Store(interviewActionLog, interviewId.FormatGuid()), Times.Never);

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IReadSideRepositoryWriter<InterviewActionLog>> interviewActionLogWriter;
        private static Mock<IDataExportService> dataExportService;
        private static Guid interviewId = Guid.NewGuid();
        private static InterviewActionLog interviewActionLog;
    }
}
