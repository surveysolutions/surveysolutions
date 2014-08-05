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
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    internal class when_TextQuestionAnsweredRecived_recived_and_previous_action_is_interviewer_assigned : InterviewExportedDataEventHandlerTestContext
    {
        Establish context = () =>
        {
            dataExportService = new Mock<IDataExportService>();
            interviewActionLogWriter = new Mock<IReadSideRepositoryWriter<InterviewActionLog>>();
            questionnarie = CreateQuestionnaireDocument(new Dictionary<string, Guid>());
            foreach (var questionAnswered in ListOfQuestionAnsweredEventsHandledByDenormalizer)
            {
                var interviewId = Guid.NewGuid();
                var interviewActionLog = new InterviewActionLog(interviewId, new List<InterviewActionExportView>());
                interviewActionLog.Actions.Add(CreateInterviewActionExportView(interviewId,
                    InterviewExportedAction.InterviewerAssigned));

                interviewActionLogWriter.Setup(x => x.GetById(interviewId.FormatGuid())).Returns(interviewActionLog);

                eventsAndInterviewActionLog.Add(interviewId,
                  new Tuple<QuestionAnswered, InterviewActionLog>(questionAnswered, interviewActionLog));
            }

            interviewExportedDataDenormalizer = CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
                () => questionnarie,
                CreateInterviewData, dataExportService.Object, new UserDocument() { UserName = "user name", Roles = new List<UserRoles>{UserRoles.Operator}}, interviewActionLogWriter.Object);
        };

        Because of = () => HandleQuestionAnsweredEventsByDenormalizer(interviewExportedDataDenormalizer, eventsAndInterviewActionLog);

        It should_FirstAnswerSet_action__be_last_in_list_of_actions_for_each_event = () => eventsAndInterviewActionLog.ShouldContainInterviewActionLog(i => i.Actions.Select(a => a.Action).Contains(InterviewExportedAction.FirstAnswerSet));

        It should_store_InterviewActionLog_for_each_event =
            () => eventsAndInterviewActionLog.ShouldCallStoreForWriter(interviewActionLogWriter, Times.Once());

        private static InterviewExportedDataDenormalizer interviewExportedDataDenormalizer;
        private static QuestionnaireDocument questionnarie;
        private static Mock<IReadSideRepositoryWriter<InterviewActionLog>> interviewActionLogWriter;
        private static Mock<IDataExportService> dataExportService;
        private static Dictionary<Guid, Tuple<QuestionAnswered, InterviewActionLog>> eventsAndInterviewActionLog = new Dictionary<Guid, Tuple<QuestionAnswered, InterviewActionLog>>();
    }
}
